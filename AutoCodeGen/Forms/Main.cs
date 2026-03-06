/*
The MIT License (MIT)

Copyright (c) 2007 Roger Hill

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files 
(the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, 
publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do 
so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN 
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using AutoCodeGenLibrary;
using DAL.Standard;
using DAL.Standard.SqlMetadata;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoCodeGen
{
    [SupportedOSPlatform("windows")]
    public partial class Main : Form
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(Main));

        private const string PRODUCT_VERSION = "3.0";
        private const string DEFAULT_SQL_CONN_STRING = "Server=(localdb)\\MSSQLLocalDB;Database=master;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;Connect Timeout=2;";

        // Encryption
        private const string PASS_PHRASE = "CodeWriter37";
        private const string INITIAL_VECTOR = "1Ds2s83a@1cw2Fg7";
        private const string SALT = "AutoCodeGenSalt";
        private const int PASSWORD_ITERATIONS = 5349;
        private const int KEY_SIZE = 256;

        /// <summary>
        /// When pulling db table names, these tables will be filtered out.
        /// </summary>
        private static readonly HashSet<string> _FilteredTableNames = new() { "master", "model", "msdb", "tempdb" };

        /// <summary>
        /// This is the maximum number of messages to keep in the messaging list view buffer.
        /// </summary>
        private const int MAX_MESSAGES = 50;

        private HashSet<string> _DirectoriesUsed = new();
        private AesEncryption _AesEncryption;
        private List<TableMetadata> _DbTables;
        private string _DatabaseName;
        private string _OutputPath;
        private List<string> _NamespaceIncludes;  // TODO add from config?
        private Dictionary<string, IOutputPlugin> _Generators;

        // Counts to help manage onChecked events for checkbox lists.
        // When event is fired, change has not been applied to object 
        // yet, so extra variables are required to track state.

        private int clbGeneratorSqlTablesCheckedCount;
        private int clbGeneratorsCheckedCount;
        private int clbExportSqlTablesCheckedCount;
        private int clbExportersCheckedCount;

        // Delegates
        private delegate void DisplayMessageSignature(string message, bool is_error);
        private delegate void DisplayUIMessageDelegate(string message);

        public Main()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                // set encryption properties
                _AesEncryption = new AesEncryption(INITIAL_VECTOR, PASSWORD_ITERATIONS, KEY_SIZE);

                Size = new Size(Properties.Settings.Default.MainFormWidth, Properties.Settings.Default.MainFormHeight);

                _DatabaseName = string.Empty;
                _OutputPath = string.Empty;
                _NamespaceIncludes = new List<string>();
                _Generators = LoadPlugins();

                // display version info
                lblVersion.Text = $"Version {PRODUCT_VERSION}";
                txtConn.Text = Properties.Settings.Default.ConnectionString;
                DisplayMessage($"{Properties.Resource.ApplicationName}, Version {PRODUCT_VERSION}", false);

                ResetApp();
                ValidateDbConnectionString();
            }
            catch (Exception ex)
            {
                if (_Log.IsErrorEnabled)
                    _Log.Error($"Main::Main() encountered an exception: {ex.Message}", ex);

                DisplayMessage(ex.Message, true);
            }
        }

        private void DisplayMessage(string message, bool isError = false)
        {
            // lose messages if over max limit
            while (rtbMessaging.Lines.Length > MAX_MESSAGES)
            {
                rtbMessaging.Select(0, rtbMessaging.Lines[0].Length + 1);
                rtbMessaging.SelectedText = string.Empty;
            }

            string formatted_time = "<" + string.Format("{0:T}", DateTime.Now) + "> ";
            string formatted_message = formatted_time + " " + message + Environment.NewLine;

            rtbMessaging.SelectionStart = rtbMessaging.TextLength;
            rtbMessaging.SelectionLength = 0;
            rtbMessaging.SelectionColor = isError ? Color.Red : Color.Black;
            rtbMessaging.AppendText(formatted_message);

            // scroll to latest
            rtbMessaging.ScrollToCaret();
        }

        /// <summary>
        /// Resets app to an initial state
        /// </summary>
        private void ResetApp()
        {
            ResetServerTab();
            ResetGeneratorTab();
            ResetExportTab();
        }

        private void ResetServerTab()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.ConnectionString))
                    txtConn.Text = _AesEncryption.Decrypt(Properties.Settings.Default.ConnectionString, PASS_PHRASE, SALT);
            }
            catch (CryptographicException)
            {
                txtConn.Text = string.Empty;
            }

            // clear all related objects
            cmbDatabaseList.DataSource = null;
            cmbDatabaseList.Items.Clear();

            clbSqlTables.Items.Clear();
            clbExportSqlTables.Items.Clear();
        }

        private void ResetGeneratorTab()
        {
            btnToggleSql.Text = Properties.Resource.SelectAll;
            btnToggleGenerators.Text = Properties.Resource.SelectAll;

            BindDataTableToCheckBoxList(_DbTables, clbSqlTables);
            clbGeneratorSqlTablesCheckedCount = clbSqlTables.CheckedItems.Count;

            clbGenerators.Items.Clear();

            // order elements before binding
            var methodBuffer = new List<string>();

            foreach (var kvp in _Generators)
                methodBuffer.Add(kvp.Key);

            clbGenerators.Items.AddRange(methodBuffer.OrderBy(i => i).ToArray());
            clbGeneratorsCheckedCount = clbGenerators.CheckedItems.Count;
        }

        private void ResetExportTab()
        {
            btnToggleExportSqlTables.Text = Properties.Resource.SelectAll;
            btnToggleExportObjects.Text = Properties.Resource.SelectAll;

            BindDataTableToCheckBoxList(_DbTables, clbExportSqlTables);
            clbExportSqlTablesCheckedCount = clbExportSqlTables.CheckedItems.Count;

            clbExportOptions.Items.Clear();
            clbExportOptions.Items.AddRange(new object[]
            {
                Properties.Resource.ExportXmlData,
                Properties.Resource.ImportXmlObject,
                Properties.Resource.ExportJsonData,
                Properties.Resource.ImportJsonObject
            });

            clbExportersCheckedCount = this.clbExportOptions.CheckedItems.Count;
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(this.txtConn.Text))
                Properties.Settings.Default.ConnectionString = _AesEncryption.Encrypt(this.txtConn.Text, PASS_PHRASE, SALT);

            Properties.Settings.Default.OutputPath = txtOutputPath.Text;
            Properties.Settings.Default.MainFormHeight = this.Size.Height;
            Properties.Settings.Default.MainFormWidth = this.Size.Width;

            Properties.Settings.Default.Save();
        }

        // Db control updates
        private void ckbLocalDb_CheckedChanged(object sender, EventArgs e)
        {
            ValidateDbConnectionString();
        }

        private void txtConn_TextChanged(object sender, EventArgs e)
        {
            ValidateDbConnectionString();
        }

        private void txtOutputPath_TextChanged(object sender, EventArgs e)
        {
            // Make sure that we only have a diretory path, and no file name(s)
            if (txtOutputPath.Text != string.Empty)
                _OutputPath = txtOutputPath.Text;
        }

        // Checkbox changes
        private void cblSqlTables_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked)
                clbGeneratorSqlTablesCheckedCount = clbSqlTables.CheckedItems.Count + 1;
            else if (e.NewValue == CheckState.Unchecked)
                clbGeneratorSqlTablesCheckedCount = clbSqlTables.CheckedItems.Count - 1;

            if (clbGeneratorSqlTablesCheckedCount == 0)
                btnToggleSql.Text = Properties.Resource.SelectAll;
            else
                btnToggleSql.Text = Properties.Resource.DeselectAll;
        }

        private void clbExportSqlTables_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked)
                clbExportSqlTablesCheckedCount = clbExportSqlTables.CheckedItems.Count + 1;
            else if (e.NewValue == CheckState.Unchecked)
                clbExportSqlTablesCheckedCount = clbExportSqlTables.CheckedItems.Count - 1;

            if (clbExportSqlTablesCheckedCount == 0)
                btnToggleExportSqlTables.Text = Properties.Resource.SelectAll;
            else
                btnToggleExportSqlTables.Text = Properties.Resource.DeselectAll;
        }

        private void clbExportOptionTables_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked)
                clbExportersCheckedCount = clbExportOptions.CheckedItems.Count + 1;
            else if (e.NewValue == CheckState.Unchecked)
                clbExportersCheckedCount = clbExportOptions.CheckedItems.Count - 1;

            if (clbExportersCheckedCount == 0)
                btnToggleExportObjects.Text = Properties.Resource.SelectAll;
            else
                btnToggleExportObjects.Text = Properties.Resource.DeselectAll;
        }

        // Button clicks
        private void btnGenerateCode_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            try
            {
                // make sure we have a solid target to push to
                if (Directory.Exists(txtOutputPath.Text))
                {
                    _OutputPath = txtOutputPath.Text;
                }
                else
                {
                    DisplayMessage("Specified output directory does not exist, halting code generation.", true);
                    return;
                }

                var sqlDatabase = new SqlDatabase();

                string file_name;

                _DirectoriesUsed.Clear();

                sqlDatabase.LoadDatabaseMetadata(_DatabaseName, txtConn.Text);

                if (clbSqlTables.CheckedItems.Count > 0 && clbGenerators.CheckedItems.Count > 0)
                {
                    var output = new List<OutputObject>();

                    foreach (string tableName in clbSqlTables.CheckedItems)
                    {
                        foreach (string method in clbGenerators.CheckedItems)
                        {
                            var generator = _Generators.ContainsKey(method) ? _Generators[method] : null;

                            if (generator != null)
                                throw new Exception("No such mapped generator");

                            SqlTable sqlTable = sqlDatabase.Tables[tableName];


                        }
                    }

                    foreach (var item in output)
                    {
                        file_name = Path.Combine(_OutputPath, item.OutputPath, item.FileName);
                        FileIo.WriteToFile(file_name, item.Body);
                        _DirectoriesUsed.Add(item.OutputPath);
                    }

                    DisplayMessage("Objects created.", false);
                }
            }
            catch (Exception ex)
            {
                if (_Log.IsErrorEnabled)
                    _Log.Error($"Main::btnGenerateCode_Click() encountered an exception: {ex.Message}", ex);

                DisplayMessage(ex.Message, true);
            }
            finally
            {
                DisplayMessage("Code generation complete.", false);
                Cursor = Cursors.Default;
                SystemSounds.Exclamation.Play();
            }
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            try
            {
                // 1) Connect to Db
                // 2) enable tabs that are now valid
                // 3) clear out db list and populate with new data

                var db = new Database(txtConn.Text);

                async Task<List<string>> processer(SqlDataReader reader)
                {
                    var output = new List<string>();

                    while (await reader.ReadAsync())
                    {
                        string buffer = (string)reader["DATABASE_NAME"];

                        if (!_FilteredTableNames.Contains(buffer))
                            output.Add(buffer);
                    }

                    return output;
                }

                var results = await db.ExecuteQueryAsync<List<string>>("[Master].[dbo].[sp_databases]", null, processer);

                if (results == null)
                {
                    // connection failed, bail
                    DisplayMessage("Failed to establish a connection to database. Please check your connection information.", true);
                    return;
                }

                DisplayMessage("Connected to server", false);

                if (results != null && results.Count != 0)
                {
                    cmbDatabaseList.BeginUpdate();
                    cmbDatabaseList.DisplayMember = "TableName";
                    cmbDatabaseList.ValueMember = "TableName";
                    cmbDatabaseList.DataSource = results;
                    cmbDatabaseList.EndUpdate();
                }
            }
            catch (Exception ex)
            {
                // 1) disable any tabs that are inappropriate
                // 2) display error message
                // 3) set connected flag to false

                if (_Log.IsErrorEnabled)
                    _Log.Error($"Main::btnConnect_Click() encountered an exception: {ex.Message}", ex);

                DisplayMessage(ex.Message, true);
                DisplayMessage("Failed to connect to server.");
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnResetCurrentTab_Click(object sender, EventArgs e)
        {
            switch (tabcontrolAutoCodeGen.SelectedTab.Name)
            {
                case "tabServer":
                    ResetServerTab();
                    break;

                case "tabSql":
                    ResetGeneratorTab();
                    break;

                case "tabExport":
                    ResetExportTab();
                    break;

                case "tabAbout":
                    break;

                default:
                    throw new Exception($"Unknown tab name '{tabcontrolAutoCodeGen.SelectedTab.Name}'");
            }
        }

        private void btnSetDirectory_Click(object sender, EventArgs e)
        {
            string output_directory = Directory.GetCurrentDirectory();
            txtOutputPath.Text = output_directory;
            DisplayMessage("Output directory set to " + output_directory, false);
        }

        private void btnOpenOutputDirectory_Click(object sender, EventArgs e)
        {
            var folderPath = txtOutputPath.Text;

            try
            {
                if (string.IsNullOrWhiteSpace(folderPath))
                {
                    DisplayMessage("Output directory name is null or empty", true);
                    return;
                }

                if (!Directory.Exists(folderPath))
                {
                    DisplayMessage($"'{folderPath}' directory does not exist", true);
                    return;
                }

                var startInfo = new ProcessStartInfo()
                {
                    Arguments = folderPath,
                    FileName = "explorer.exe"
                };

                Process.Start(startInfo);
            }
            catch
            {
                DisplayMessage($"Error accessing file path '{folderPath}'", true);
            }
        }

        // select/deselect all code
        private void btnToggleSql_Click(object sender, EventArgs e)
        {
            if (btnToggleSql.Text == Properties.Resource.SelectAll)
            {
                btnToggleSql.Text = Properties.Resource.DeselectAll;
                ToggleCheckBoxes(clbSqlTables, true);
            }
            else
            {
                btnToggleSql.Text = Properties.Resource.SelectAll;
                ToggleCheckBoxes(clbSqlTables, false);
            }
        }

        private void btnToggleGenerators_Click(object sender, EventArgs e)
        {
            if (btnToggleGenerators.Text == Properties.Resource.SelectAll)
            {
                btnToggleGenerators.Text = Properties.Resource.DeselectAll;
                ToggleCheckBoxes(clbGenerators, true);
            }
            else
            {
                btnToggleGenerators.Text = Properties.Resource.SelectAll;
                ToggleCheckBoxes(clbGenerators, false);
            }
        }

        private void btnToggleExportSqlTables_Click(object sender, EventArgs e)
        {
            if (btnToggleExportSqlTables.Text == Properties.Resource.SelectAll)
            {
                btnToggleExportSqlTables.Text = Properties.Resource.DeselectAll;
                ToggleCheckBoxes(clbExportSqlTables, true);
            }
            else
            {
                btnToggleExportSqlTables.Text = Properties.Resource.SelectAll;
                ToggleCheckBoxes(clbExportSqlTables, false);
            }
        }

        private void btnToggleExportObjects_Click(object sender, EventArgs e)
        {
            if (btnToggleGenerators.Text == Properties.Resource.SelectAll)
            {
                btnToggleGenerators.Text = Properties.Resource.DeselectAll;
                ToggleCheckBoxes(clbExportOptions, true);
            }
            else
            {
                btnToggleGenerators.Text = Properties.Resource.SelectAll;
                ToggleCheckBoxes(clbExportOptions, false);
            }
        }

        // Other events
        private async void cmbDatabaseList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            try
            {
                await GetDbTables();

                // reset all tabs that have table specific data
                ResetGeneratorTab();
                ResetExportTab();
            }
            catch (Exception ex)
            {
                if (_Log.IsErrorEnabled)
                    _Log.Error($"Main::cmbDatabaseList_SelectedIndexChanged() encountered an exception: {ex.Message}", ex);

                DisplayMessage(ex.Message, true);
            }
            finally
            {
                DisplayMessage("Changed to database " + cmbDatabaseList.Text, false);
                Cursor = Cursors.Default;
            }
        }

        private void lnkWebLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("IExplore", Properties.Resource.WebsiteUrl);
            }
            catch (Exception ex)
            {
                DisplayMessage("Cannot find browser. Please open your browser manually.", true);

                if (_Log.IsErrorEnabled)
                    _Log.Error($"Main::lnkWebLink_LinkClicked() encountered an exception: {ex.Message}", ex);
            }
        }

        private void lnkEmail_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("mailto:" + Properties.Resource.AuthorEmailAddress);
            }
            catch (Exception ex)
            {
                DisplayMessage("Cannot find e-mail application. Please open your e-mail client manually.", true);

                if (_Log.IsErrorEnabled)
                    _Log.Error($"Main::lnkEmail_LinkClicked() encountered an exception: {ex.Message}", ex);
            }
        }

        // helper functions

        private void BindDataTableToCheckBoxList(List<TableMetadata> tables, CheckedListBox clb)
        {
            if (clb.Items.Count != 0)
                clb.Items.Clear();

            if (tables == null || tables.Count < 1)
                return;

            foreach (var table in tables)
            {
                clb.Items.AddRange(new object[] { $"{table.Schema}.{table.TableName}" });
            }
        }

        /// <summary>
        /// function takes the name of the current Db that we are working with,
        /// and populates the _DbTablesList with the names of the tables in that
        /// database. NOTE that data tables list is not currently used for anything. 
        /// </summary>
        private async Task GetDbTables()
        {
            _DatabaseName = cmbDatabaseList.Text;

            if (string.IsNullOrEmpty(_DatabaseName))
            {
                DisplayMessage("Database name is null or empty, cannot load table data", true);
                return;
            }

            var db = new Database(txtConn.Text);

            string sql_query = $"[{_DatabaseName}].[dbo].[sp_tables] null,null,null,\"'TABLE'\"";

            async Task<List<TableMetadata>> processor(SqlDataReader reader)
            {
                var output = new List<TableMetadata>();

                while (await reader.ReadAsync())
                {
                    var buffer = new TableMetadata
                    {
                        DbName = (string)reader["TABLE_QUALIFIER"],
                        Schema = (string)reader["TABLE_OWNER"],
                        TableName = (string)reader["TABLE_NAME"],
                        TableType = (string)reader["TABLE_TYPE"]
                    };

                    output.Add(buffer);
                }

                return output.OrderBy(t => t.Schema).ThenBy(t => t.TableName).ToList();
            }

            var buffer = await db.ExecuteQueryAsync(sql_query, null, processor);

            _DbTables = buffer
                .Where(c => c.Schema != "sys" && c.TableName != "sysdiagrams")
                .OrderBy(c => c.TableName)
                .ToList();
        }

        private void ToggleCheckBoxes(CheckedListBox clb, bool newCheckedState)
        {
            // Sets CheckBoxLists to passed value
            for (int i = 0; i < clb.Items.Count; i++)
                clb.SetItemChecked(i, newCheckedState);
        }

        /// <summary>
        /// we checked or unchecked the use local database option, update UI to reflect new options.
        /// </summary>
        private void ValidateDbConnectionString()
        {
            btnConnect.Enabled = !string.IsNullOrWhiteSpace(txtConn.Text);
        }

        private void UpdateComboBoxList(ComboBox input, List<string> list)
        {
            _NamespaceIncludes.Sort();

            input.Items.Clear();

            foreach (var item in list)
                input.Items.Add(item);
        }

        /// <summary>
        /// Scans the assembly for all available plugins that implement IPlugin interface.
        /// returns a dictionary of plugin names and plugin instances.
        /// </summary>
        private Dictionary<string, IOutputPlugin> LoadPlugins()
        {
            var pluginType = typeof(IOutputPlugin);
            var assembly = typeof(IOutputPlugin).Assembly;

            var buffer = assembly
                .GetTypes()
                .Where(t =>
                    t.IsClass &&
                    !t.IsAbstract &&
                    pluginType.IsAssignableFrom(t) &&
                    t.Namespace != null &&
                    t.Namespace.StartsWith("AutoCodeGenLibrary"))
                .Select(t => (IOutputPlugin)Activator.CreateInstance(t)!);

            var output = new Dictionary<string, IOutputPlugin>();

            foreach (var plugin in buffer)
            {
                output.Add(plugin.Name, plugin);
            }

            return output;
        }

        private void btnUseDefaultConn_Click(object sender, EventArgs e)
        {
            txtConn.Text = DEFAULT_SQL_CONN_STRING;
            ValidateDbConnectionString();
        }

        private void btnCleanOutput_Click(object sender, EventArgs e)
        {
            try
            {
                //string applicationPath = Path.GetDirectoryName(Application.ExecutablePath);

                if (_OutputPath == null || !Directory.Exists(_OutputPath))
                    return;

                var fileList = Directory.GetFiles(_OutputPath, "*.*", SearchOption.AllDirectories);

                foreach (string filename in fileList)
                    File.Delete(filename);

                var directoryList = Directory.GetDirectories(_OutputPath, "*.*", SearchOption.TopDirectoryOnly);

                foreach (string directoryName in directoryList)
                    Directory.Delete(directoryName, false);
            }
            catch
            {
                DisplayMessage($"Error encountered in removing script files from {_OutputPath}. Please make sure they aren't open in other programs.", true);
                return;
            }

            DisplayMessage($"Script files removed from {_OutputPath}", false);
        }

        //private DataTable LoadSqlTableData(SqlTable sqlTable, string connectionString)
        //{
        //    if (sqlTable == null)
        //        throw new ArgumentException("SqlTable is null");

        //    string query = $"SELECT * FROM [{sqlTable.Database.Name}].[{sqlTable.Schema}].[{sqlTable.Name}]";
        //    var db = new Database(connectionString);

        //    DataTable data_table = db.ExecuteQuery(query, null);
        //    data_table.TableName = sqlTable.Name;

        //    return data_table;
        //}
    }
}