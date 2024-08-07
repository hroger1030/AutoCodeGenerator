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
using Encryption;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace AutoCodeGen
{
    public partial class Main : Form
    {
        private static string PRODUCT_VERSION = "2.0";

        private static readonly ILog _Log = LogManager.GetLogger(typeof(Main));

        // output directories
        internal static string _DirectorySql = ConfigurationManager.AppSettings["DirectorySql"];
        internal static string _DirectoryReactPages = ConfigurationManager.AppSettings["DirectoryReactPages"];
        internal static string _DirectoryOrm = ConfigurationManager.AppSettings["DirectoryOrm"];
        internal static string _DirectoryOrmExt = ConfigurationManager.AppSettings["DirectoryOrmExt"];
        internal static string _DirectoryDal = ConfigurationManager.AppSettings["DirectoryDal"];
        internal static string _DirectoryDalExt = ConfigurationManager.AppSettings["DirectoryDalExt"];
        internal static string _DirectoryWebService = ConfigurationManager.AppSettings["DirectoryWebService"];
        internal static string _DirectoryInterface = ConfigurationManager.AppSettings["DirectoryInterface"];
        internal static string _DirectoryExportData = ConfigurationManager.AppSettings["DirectoryExportData"];
        internal static string _DirectoryEnums = ConfigurationManager.AppSettings["DirectoryEnums"];

        // Encryption
        private static readonly string _PassPhrase = "CodeWriter37";
        private static readonly string _InitialVector = "1Ds2s83a@1cw2Fg7";
        private static readonly string _Salt = "AutoCodeGenSalt";

        // Misc
        private static readonly string[] _FileExtensionsUsed = { "*.sql", "*.cs", "*.xml", "*.ts", "*.tsx", "*.json", "*.css" };
        private static readonly string[] _DirectoriesUsed = { _DirectoryReactPages, _DirectoryOrm, _DirectoryOrmExt, _DirectoryDal, _DirectoryDalExt, _DirectoryWebService, _DirectoryInterface, _DirectoryExportData, _DirectoryEnums };
        private static readonly HashSet<string> _FilteredTableNames = new() { "master", "model", "msdb", "tempdb" };

        private const int MAX_MESSAGES = 50;

        private readonly AesEncryption _AesEncryption;
        private string _Conn;
        private List<TableMetadata> _DbTables;
        private string _DatabaseName;
        private string _OutputPath;
        private readonly List<string> _NamespaceIncludes;
        //private readonly HashSet<IGenerator> _RegisteredGenerators;

        // Counts to help manage onChecked events for checkboxlists.
        // When event is fired, change has not been applied to object 
        // yet, so extra vars are required to track state.

        private int clbTsqlObjectsCheckedCount;
        private int clbCsharpObjectsCheckedCount;
        private int clbWebServiceObjectsCheckedCount;
        private int clbReactbjectsCheckedCount;
        private int clbOutputOptionsCheckedCount;

        private int clbTsqlSqlTablesCheckedCount;
        private int clbCsharpSqlTablesCheckedCount;
        private int clbWebServiceSqlTablesCheckedCount;
        private int clbReactSqlTablesCheckedCount;
        private int clbExportSqlTablesCheckedCount;
        private int clbJsonOptionsTablesCheckedCount;

        // Delegates
        private delegate void DisplayMessageSignature(string message, bool is_error);
        private delegate void DisplayUIMessageDelegate(string message);

        public Main()
        {
            try
            {
                InitializeComponent();

                // set encryption properties
                _AesEncryption = new AesEncryption(_InitialVector, 5349, 256);

                Size = new Size(Properties.Settings.Default.MainFormWidth, Properties.Settings.Default.MainFormHeight);

                _DatabaseName = string.Empty;
                _OutputPath = string.Empty;
                _NamespaceIncludes = new List<string>();
                //_RegisteredGenerators = new HashSet<IGenerator>
                //{
                //    new CodeGeneratorCSharp(),
                //    new CodeGeneratorSql(),
                //    new CodeGeneratorAspDotNet(),
                //    new CodeGeneratorWebservice(),
                //    new CodeGeneratorWinform(),
                //};

                // display version info
                lblVersion.Text = $"Version {PRODUCT_VERSION}";
                DisplayMessage($"{Properties.Resource.ApplicationName}, Version {PRODUCT_VERSION}", false);

                ResetApp();
                ValidateDbConnectionString();
                ValidateTab();
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
            if (lvMessaging.InvokeRequired)
            {
                var display_message = new DisplayMessageSignature(DisplayMessage);
                lvMessaging.Invoke(display_message, new object[] { message, isError });
            }
            else
            {
                // lose messages if over max limit
                while (lvMessaging.Items.Count > MAX_MESSAGES)
                    lvMessaging.Items.RemoveAt(0);

                var list_view_item = new ListViewItem();
                string formatted_time = "<" + string.Format("{0:T}", DateTime.Now) + "> ";

                if (isError)
                    list_view_item.ForeColor = Color.Red;
                else
                    list_view_item.ForeColor = Color.Black;

                list_view_item.Text = formatted_time + " " + message;

                lvMessaging.Items.Add(list_view_item);
                lvMessaging.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
                lvMessaging.SelectedItems.Clear();
                lvMessaging.Items[lvMessaging.Items.Count - 1].Selected = true;
                lvMessaging.Items[lvMessaging.Items.Count - 1].EnsureVisible();
            }
        }

        private void ResetApp()
        {
            // Resets app to initial state
            ResetServerTab();
            ResetSqlTab();
            ResetCsharpTab();
            ResetWebServiceTab();
            ResetReactTab();
            ResetExportTab();
            ResetOutputTab();
        }

        private void ResetServerTab()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.ServerName))
                    txtServerName.Text = _AesEncryption.Decrypt(Properties.Settings.Default.ServerName, _PassPhrase, _Salt);

                if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.UserName))
                    txtLogon.Text = _AesEncryption.Decrypt(Properties.Settings.Default.UserName, _PassPhrase, _Salt);

                if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.Password))
                    txtPassword.Text = _AesEncryption.Decrypt(Properties.Settings.Default.Password, _PassPhrase, _Salt);
            }
            catch (CryptographicException)
            {
                txtServerName.Text = string.Empty;
                txtLogon.Text = string.Empty;
                txtPassword.Text = string.Empty;
            }

            ckbLocalDb.Checked = Properties.Settings.Default.LocalDb;
            _Conn = ConfigurationManager.ConnectionStrings["SQLConnection"].ToString();

            // clear all related objects
            cmbDatabaseList.DataSource = null;
            cmbDatabaseList.Items.Clear();

            lblConnectStatus.Text = Properties.Resource.ConnectionMessage;

            clbTsqlSqlTables.Items.Clear();
            clbCsharpSqlTables.Items.Clear();
            clbWebServiceSqlTables.Items.Clear();
            clbReactSqlTables.Items.Clear();
            clbExportSqlTables.Items.Clear();
        }

        private void ResetSqlTab()
        {
            btnToggleTsqlSqlTables.Text = Properties.Resource.SelectAll;
            btnToggleTsqlSqlObjects.Text = Properties.Resource.SelectAll;

            BindDataTableToCheckBoxList(_DbTables, clbTsqlSqlTables);
            clbTsqlSqlTablesCheckedCount = clbTsqlSqlTables.CheckedItems.Count;

            clbTsqlSqlObjects.Items.Clear();
            clbTsqlSqlObjects.Items.AddRange(new object[]
            {
                Properties.Resource.SqlSelSingle,
                Properties.Resource.SqlSelMany,
                Properties.Resource.SqlSelManyByX,
                Properties.Resource.SqlSelAll,
                Properties.Resource.SqlSearchPaged,
                Properties.Resource.SqlUpd,
                Properties.Resource.SqlIns,
                Properties.Resource.SqlUpdIns,
                Properties.Resource.SqlDelSingle,
                Properties.Resource.SqlDelMany,
                Properties.Resource.SqlDelAll,
            });

            clbTsqlObjectsCheckedCount = clbTsqlSqlObjects.CheckedItems.Count;
        }

        private void ResetCsharpTab()
        {
            btnToggleCsharpSqlTables.Text = Properties.Resource.SelectAll;
            btnToggleCSharpObjects.Text = Properties.Resource.SelectAll;

            BindDataTableToCheckBoxList(_DbTables, clbCsharpSqlTables);
            clbCsharpSqlTablesCheckedCount = clbCsharpSqlTables.CheckedItems.Count;

            clbCsharpObjects.Items.Clear();
            clbCsharpObjects.Items.AddRange(new CodeGeneratorCSharp().Methods.Keys.ToArray<object>());
            clbCsharpObjectsCheckedCount = clbCsharpObjects.CheckedItems.Count;
        }

        private void ResetWebServiceTab()
        {
            btnToggleWebServicesSqlTables.Text = Properties.Resource.SelectAll;
            btnToggleWebServiceObjects.Text = Properties.Resource.SelectAll;

            BindDataTableToCheckBoxList(_DbTables, clbWebServiceSqlTables);
            clbWebServiceSqlTablesCheckedCount = clbWebServiceSqlTables.CheckedItems.Count;

            clbWebServiceObjects.Items.Clear();
            clbWebServiceObjects.Items.AddRange(new object[]
            {
                Properties.Resource.WebServiceController,
                Properties.Resource.WebServiceControllerBase,
                Properties.Resource.WebServiceResponseObject,
                Properties.Resource.WebServicePagingObject,
            });

            clbWebServiceObjectsCheckedCount = clbWebServiceObjects.CheckedItems.Count;
        }

        private void ResetReactTab()
        {
            btnToggleReactSqlTables.Text = Properties.Resource.SelectAll;
            btnToggleASPObjects.Text = Properties.Resource.SelectAll;

            BindDataTableToCheckBoxList(_DbTables, clbReactSqlTables);
            clbReactSqlTablesCheckedCount = clbReactSqlTables.CheckedItems.Count;

            clbReactObjects.Items.Clear();
            clbReactObjects.Items.AddRange(new object[]
            {
                Properties.Resource.ReactCreateEditPage,
                Properties.Resource.ReactCreateListPage,
                Properties.Resource.ReactCreateViewPage
            });

            clbCsharpObjectsCheckedCount = clbCsharpObjects.CheckedItems.Count;
        }

        private void ResetExportTab()
        {
            btnToggleExportSqlTables.Text = Properties.Resource.SelectAll;
            btnToggleExportObjects.Text = Properties.Resource.SelectAll;

            BindDataTableToCheckBoxList(_DbTables, clbExportSqlTables);
            clbExportSqlTablesCheckedCount = clbExportSqlTables.CheckedItems.Count;

            clbExportOptionsTables.Items.Clear();
            clbExportOptionsTables.Items.AddRange(new object[]
            {
                Properties.Resource.ExportXmlData,
                Properties.Resource.ImportXmlObject,
                Properties.Resource.ExportJsonData,
                Properties.Resource.ImportJsonObject
            });

            clbJsonOptionsTablesCheckedCount = this.clbExportOptionsTables.CheckedItems.Count;
        }

        private void ResetOutputTab()
        {
            // get output path from registry, and format it
            string OutputPath = Properties.Settings.Default.OutputPath;

            if (OutputPath != null && Directory.Exists(OutputPath))
                txtOutputPath.Text = OutputPath;
            else
                txtOutputPath.Text = Directory.GetCurrentDirectory();

            txtTableNameRegex.Text = Properties.Settings.Default.TableNameRegex;
            btnToggleOutput.Text = Properties.Resource.SelectAll;

            // Set up output options box
            clbOutputOptions.Items.Clear();
            clbOutputOptions.Items.AddRange(new object[]
            {
                Properties.Resource.OptSQLCreateHelperSp,
                Properties.Resource.OptSQLCreateSqlSpPerms,
                Properties.Resource.OptSQLSeperateFiles,

                Properties.Resource.OptMiscRemoveExistingScripts,

                Properties.Resource.OptCsharpConvertNullableFields,
                Properties.Resource.OptCsharpIncludeSqlClassDecoration,
                Properties.Resource.OptCsharpIncludeIsDirtyFlag,

                Properties.Resource.OptXmlFormat,
                Properties.Resource.OptXmlIncludeNs,
            });

            GetOutputFlagSettings();

            clbOutputOptionsCheckedCount = clbOutputOptions.CheckedItems.Count;
        }

        // UI Label changes
        private void SetConnectStatusLabel(string message)
        {
            if (lblConnectStatus.InvokeRequired)
                lblConnectStatus.Invoke(new DisplayUIMessageDelegate(SetConnectStatusLabel), new object[] { message });
            else
                lblConnectStatus.Text = message;
        }

        // helper functions
        private void OpenFolder(string folderPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(folderPath))
                {
                    MessageBox.Show($"Directory name cannot be null or empty");
                    return;
                }

                if (!Directory.Exists(folderPath))
                {
                    MessageBox.Show($"{folderPath} Directory does not exist!");
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

        private bool DeleteOldScriptFiles()
        {
            string application_path = Path.GetDirectoryName(Application.ExecutablePath);

            try
            {
                foreach (string file_extension in _FileExtensionsUsed)
                {
                    string[] delete_list = Directory.GetFiles(_OutputPath, file_extension);

                    foreach (string file_name in delete_list)
                        File.Delete(file_name);
                }

                foreach (string folder_name in _DirectoriesUsed)
                {
                    if (Directory.Exists(application_path + folder_name))
                        FileIo.DeleteDirectoryTree(_OutputPath + folder_name);
                }
            }
            catch
            {
                DisplayMessage($"Error encountered in removing script files from {_OutputPath}. Please make sure they aren't open in other programs.", true);
                return false;
            }

            DisplayMessage($"Script files removed from {_OutputPath}", false);
            return true;
        }

        private void GetDbTables()
        {
            // function takes the name of the current Db that we are working with,
            // and populates the _DbTablesList with the names of the tables in that
            // database. NOTE that datatables list is not currently used for anything.

            _DatabaseName = cmbDatabaseList.Text;

            if (string.IsNullOrEmpty(_DatabaseName))
                throw new ArgumentException("Database name is null or empty");

            var db = new Database(_Conn);

            string sql_query = $"[{_DatabaseName}].[dbo].[sp_tables] null,null,null,\"'TABLE'\"";

            List<TableMetadata> processer(SqlDataReader reader)
            {
                var output = new List<TableMetadata>();

                while (reader.Read())
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

            _DbTables = db.ExecuteQuery(sql_query, null, processer)
                .Where(c => c.Schema != "sys" && c.TableName != "sysdiagrams")
                .OrderBy(c => c.TableName)
                .ToList();
        }

        private void GetOutputFlagSettings()
        {
            // Read in output flag settings from properties object

            if (Properties.Settings.Default.RemoveExistingScripts) CheckCheckboxListItem(Properties.Resource.OptMiscRemoveExistingScripts);
            if (Properties.Settings.Default.CreateHelperSp) CheckCheckboxListItem(Properties.Resource.OptSQLCreateHelperSp);
            if (Properties.Settings.Default.CreateSqlSpPerms) CheckCheckboxListItem(Properties.Resource.OptSQLCreateSqlSpPerms);
            if (Properties.Settings.Default.SqlSeperateFiles) CheckCheckboxListItem(Properties.Resource.OptSQLSeperateFiles);

            if (Properties.Settings.Default.CsIncludeIsDirtyFlag) CheckCheckboxListItem(Properties.Resource.OptCsharpIncludeIsDirtyFlag);
            if (Properties.Settings.Default.CsCreateBaseClass) CheckCheckboxListItem(Properties.Resource.CsharpCreateBaseClass);
            if (Properties.Settings.Default.CsIncludeSqlClassDecoration) CheckCheckboxListItem(Properties.Resource.OptCsharpIncludeSqlClassDecoration);
            if (Properties.Settings.Default.CsIncludeBaseClassRefrence) CheckCheckboxListItem(Properties.Resource.OptCsharpIncludeBaseClassRefrence);
            if (Properties.Settings.Default.CsConvertNullableFields) CheckCheckboxListItem(Properties.Resource.OptCsharpConvertNullableFields);

            if (Properties.Settings.Default.XmlFormat) CheckCheckboxListItem(Properties.Resource.OptXmlFormat);
            if (Properties.Settings.Default.XmlIncludeNs) CheckCheckboxListItem(Properties.Resource.OptXmlIncludeNs);
        }

        private void SetOutputFlagSettings()
        {
            // write current checkbox setting in to properties

            Properties.Settings.Default.RemoveExistingScripts = clbOutputOptions.CheckedItems.Contains(Properties.Resource.OptMiscRemoveExistingScripts);
            Properties.Settings.Default.CreateHelperSp = clbOutputOptions.CheckedItems.Contains(Properties.Resource.OptSQLCreateHelperSp);
            Properties.Settings.Default.CreateSqlSpPerms = clbOutputOptions.CheckedItems.Contains(Properties.Resource.OptSQLCreateSqlSpPerms);
            Properties.Settings.Default.SqlSeperateFiles = clbOutputOptions.CheckedItems.Contains(Properties.Resource.OptSQLSeperateFiles);

            Properties.Settings.Default.CsIncludeIsDirtyFlag = clbOutputOptions.CheckedItems.Contains(Properties.Resource.OptCsharpIncludeIsDirtyFlag);
            Properties.Settings.Default.CsCreateBaseClass = clbOutputOptions.CheckedItems.Contains(Properties.Resource.CsharpCreateBaseClass);
            Properties.Settings.Default.CsIncludeSqlClassDecoration = clbOutputOptions.CheckedItems.Contains(Properties.Resource.OptCsharpIncludeSqlClassDecoration);
            Properties.Settings.Default.CsIncludeBaseClassRefrence = clbOutputOptions.CheckedItems.Contains(Properties.Resource.OptCsharpIncludeBaseClassRefrence);
            Properties.Settings.Default.CsConvertNullableFields = clbOutputOptions.CheckedItems.Contains(Properties.Resource.OptCsharpConvertNullableFields);

            Properties.Settings.Default.XmlFormat = clbOutputOptions.CheckedItems.Contains(Properties.Resource.OptXmlFormat);
            Properties.Settings.Default.XmlIncludeNs = clbOutputOptions.CheckedItems.Contains(Properties.Resource.OptXmlIncludeNs);

            Properties.Settings.Default.Save();
        }

        private void ToggleCheckBoxes(CheckedListBox clb, bool newchecked_state)
        {
            // Sets CheckBoxLists to passed value
            for (int i = 0; i < clb.Items.Count; i++)
                clb.SetItemChecked(i, newchecked_state);
        }

        private void ValidateDbConnectionString()
        {
            // we checked or unchecked the use local database option,
            // update UI to reflect new options.

            if (ckbLocalDb.Checked == true)
            {
                txtLogon.Enabled = false;
                txtPassword.Enabled = false;
                txtServerName.Enabled = false;
                btnConnect.Enabled = true;

                _Conn = ConfigurationManager.ConnectionStrings["SQLConnection"].ToString();

                return;
            }
            else
            {
                txtLogon.Enabled = true;
                txtPassword.Enabled = true;
                txtServerName.Enabled = true;

                _Conn = $"Server={txtServerName.Text};Database={cmbDatabaseList.SelectedValue};User Id={txtLogon.Text};Password={txtPassword.Text};Connection Timeout=2;";

                if (!string.IsNullOrWhiteSpace(txtPassword.Text) && !string.IsNullOrWhiteSpace(txtLogon.Text) && !string.IsNullOrWhiteSpace(txtServerName.Text))
                {
                    btnConnect.Enabled = true;
                    return;
                }
            }

            btnConnect.Enabled = false;
        }

        private void ValidateTab()
        {
            // validates tab data to determine if we are ready to generate code. 
            // if any tab has some valid selections in place, enable code generation

            // SQL tab
            if (clbTsqlObjectsCheckedCount > 0 && clbTsqlSqlTablesCheckedCount > 0)
            {
                btnGenerateCode.Enabled = true;
                return;
            }

            // CSharp Tab
            if (clbCsharpObjectsCheckedCount > 0 && clbCsharpSqlTablesCheckedCount > 0)
            {
                btnGenerateCode.Enabled = true;
                return;
            }

            // WebService Tab
            if (clbWebServiceSqlTablesCheckedCount > 0 && clbWebServiceObjectsCheckedCount > 0)
            {
                btnGenerateCode.Enabled = true;
                return;
            }

            // React Tab
            if (clbReactbjectsCheckedCount > 0 && clbReactSqlTablesCheckedCount > 0)
            {
                btnGenerateCode.Enabled = true;
                return;
            }

            // XML tab
            if (clbExportSqlTablesCheckedCount > 0 && clbJsonOptionsTablesCheckedCount > 0)
            {
                btnGenerateCode.Enabled = true;
                return;
            }

            // Output tab
            if (clbOutputOptionsCheckedCount > 0)
            {
                btnGenerateCode.Enabled = true;
                return;
            }

            // no matches, disable button
            btnGenerateCode.Enabled = false;
        }

        private void UpdateComboBoxList(ComboBox input, List<string> list)
        {
            _NamespaceIncludes.Sort();

            input.Items.Clear();

            foreach (var item in list)
                input.Items.Add(item);
        }

        private bool CheckCheckboxListItem(string search_string)
        {
            int listbox_index = clbOutputOptions.FindStringExact(search_string);

            if (listbox_index == -1)
                return false;

            clbOutputOptions.SetItemChecked(listbox_index, true);
            return true;
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(this.txtServerName.Text))
                Properties.Settings.Default.ServerName = _AesEncryption.Encrypt(this.txtServerName.Text, _PassPhrase, _Salt);

            if (!string.IsNullOrWhiteSpace(this.txtLogon.Text))
                Properties.Settings.Default.UserName = _AesEncryption.Encrypt(this.txtLogon.Text, _PassPhrase, _Salt);

            if (!string.IsNullOrWhiteSpace(this.txtPassword.Text))
                Properties.Settings.Default.Password = _AesEncryption.Encrypt(this.txtPassword.Text, _PassPhrase, _Salt);

            Properties.Settings.Default.LocalDb = ckbLocalDb.Checked;
            Properties.Settings.Default.OutputPath = txtOutputPath.Text;
            Properties.Settings.Default.MainFormHeight = this.Size.Height;
            Properties.Settings.Default.MainFormWidth = this.Size.Width;
            Properties.Settings.Default.TableNameRegex = txtTableNameRegex.Text;

            Properties.Settings.Default.Save();

            // persists all the checkbox states in the options screen
            SetOutputFlagSettings();
        }

        // Db control updates
        private void ckbLocalDb_CheckedChanged(object sender, EventArgs e)
        {
            ValidateDbConnectionString();
            ValidateTab();
        }

        private void txtLogon_TextChanged(object sender, EventArgs e)
        {
            ValidateDbConnectionString();
            ValidateTab();
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {
            ValidateDbConnectionString();
            ValidateTab();
        }

        private void txtServerName_TextChanged(object sender, EventArgs e)
        {
            if (ckbLocalDb.Checked == false && txtServerName.Text != string.Empty)
            {
                ValidateDbConnectionString();
                ValidateTab();
            }
        }

        private void txtOutputPath_TextChanged(object sender, EventArgs e)
        {
            // Make sure that we only have a diretory path, and no file name(s)
            if (txtOutputPath.Text != string.Empty)
                _OutputPath = txtOutputPath.Text;
        }

        // Checkbox changes
        private void clbTsqlSqlTables_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked)
                clbTsqlSqlTablesCheckedCount = clbTsqlSqlTables.CheckedItems.Count + 1;
            else if (e.NewValue == CheckState.Unchecked)
                clbTsqlSqlTablesCheckedCount = clbTsqlSqlTables.CheckedItems.Count - 1;

            if (clbTsqlSqlTablesCheckedCount == 0)
                btnToggleTsqlSqlTables.Text = Properties.Resource.SelectAll;
            else
                btnToggleTsqlSqlTables.Text = Properties.Resource.DeselectAll;

            ValidateTab();
        }

        private void clbTsqlSqlObjects_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked)
                clbTsqlObjectsCheckedCount = clbTsqlSqlObjects.CheckedItems.Count + 1;
            else if (e.NewValue == CheckState.Unchecked)
                clbTsqlObjectsCheckedCount = clbTsqlSqlObjects.CheckedItems.Count - 1;

            if (clbTsqlObjectsCheckedCount == 0)
                btnToggleTsqlSqlObjects.Text = Properties.Resource.SelectAll;
            else
                btnToggleTsqlSqlObjects.Text = Properties.Resource.DeselectAll;

            ValidateTab();
        }

        private void clbCsharpSqlTables_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked)
                clbCsharpSqlTablesCheckedCount = clbCsharpSqlTables.CheckedItems.Count + 1;
            else if (e.NewValue == CheckState.Unchecked)
                clbCsharpSqlTablesCheckedCount = clbCsharpSqlTables.CheckedItems.Count - 1;

            if (clbCsharpSqlTablesCheckedCount == 0)
                btnToggleWebServicesSqlTables.Text = Properties.Resource.SelectAll;
            else
                btnToggleWebServicesSqlTables.Text = Properties.Resource.DeselectAll;

            ValidateTab();
        }

        private void clbCSharpObjects_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked)
                clbCsharpObjectsCheckedCount = clbCsharpObjects.CheckedItems.Count + 1;
            else if (e.NewValue == CheckState.Unchecked)
                clbCsharpObjectsCheckedCount = clbCsharpObjects.CheckedItems.Count - 1;

            if (clbCsharpObjectsCheckedCount == 0)
                btnToggleCSharpObjects.Text = Properties.Resource.SelectAll;
            else
                btnToggleCSharpObjects.Text = Properties.Resource.DeselectAll;

            ValidateTab();
        }

        private void clbWebServiceSqlTables_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked)
                clbWebServiceSqlTablesCheckedCount = clbWebServiceSqlTables.CheckedItems.Count + 1;
            else if (e.NewValue == CheckState.Unchecked)
                clbWebServiceSqlTablesCheckedCount = clbWebServiceSqlTables.CheckedItems.Count - 1;

            if (clbWebServiceSqlTablesCheckedCount == 0)
                btnToggleWebServicesSqlTables.Text = Properties.Resource.SelectAll;
            else
                btnToggleWebServicesSqlTables.Text = Properties.Resource.DeselectAll;

            ValidateTab();
        }

        private void clbWebServiceObjects_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked)
                clbWebServiceObjectsCheckedCount = clbWebServiceObjects.CheckedItems.Count + 1;
            else if (e.NewValue == CheckState.Unchecked)
                clbWebServiceObjectsCheckedCount = clbWebServiceObjects.CheckedItems.Count - 1;

            if (clbWebServiceObjectsCheckedCount == 0)
                btnToggleWebServiceObjects.Text = Properties.Resource.SelectAll;
            else
                btnToggleWebServiceObjects.Text = Properties.Resource.DeselectAll;

            ValidateTab();
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

            ValidateTab();
        }

        private void clbExportOptionTables_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked)
                clbJsonOptionsTablesCheckedCount = clbExportOptionsTables.CheckedItems.Count + 1;
            else if (e.NewValue == CheckState.Unchecked)
                clbJsonOptionsTablesCheckedCount = clbExportOptionsTables.CheckedItems.Count - 1;

            if (clbJsonOptionsTablesCheckedCount == 0)
                btnToggleExportObjects.Text = Properties.Resource.SelectAll;
            else
                btnToggleExportObjects.Text = Properties.Resource.DeselectAll;

            ValidateTab();
        }

        private void clbReactObjects_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked)
                clbReactbjectsCheckedCount = clbReactObjects.CheckedItems.Count + 1;
            else if (e.NewValue == CheckState.Unchecked)
                clbReactSqlTablesCheckedCount = clbReactObjects.CheckedItems.Count - 1;

            if (clbReactbjectsCheckedCount == 0)
                btnToggleASPObjects.Text = Properties.Resource.SelectAll;
            else
                btnToggleASPObjects.Text = Properties.Resource.DeselectAll;

            ValidateTab();
        }

        private void clbReactSqlTables_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked)
                clbReactSqlTablesCheckedCount = clbReactSqlTables.CheckedItems.Count + 1;
            else if (e.NewValue == CheckState.Unchecked)
                clbReactSqlTablesCheckedCount = clbReactSqlTables.CheckedItems.Count - 1;

            if (clbReactSqlTablesCheckedCount == 0)
                btnToggleReactSqlTables.Text = Properties.Resource.SelectAll;
            else
                btnToggleReactSqlTables.Text = Properties.Resource.DeselectAll;

            ValidateTab();
        }

        private void clbOutputOptions_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked)
                clbOutputOptionsCheckedCount = clbOutputOptions.CheckedItems.Count + 1;
            else if (e.NewValue == CheckState.Unchecked)
                clbOutputOptionsCheckedCount = clbOutputOptions.CheckedItems.Count - 1;

            if (clbOutputOptionsCheckedCount == 0)
                btnToggleOutput.Text = Properties.Resource.SelectAll;
            else
                btnToggleOutput.Text = Properties.Resource.DeselectAll;

            ValidateTab();
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

                var sql_database = new SqlDatabase();
                var sb = new StringBuilder();
                var output = new OutputObject();

                string regex_string = txtTableNameRegex.Text;
                string output_file_name = string.Empty;
                string generated_code = string.Empty;
                string file_name = string.Empty;

                //////////////////////////////////////////////////////////////////////

                //temp hack

                var codegeneratorCsharp = new CodeGeneratorCSharp();
                var codegeneratorSql = new CodeGeneratorSql();
                var codegeneratorRest = new CodeGeneratorWebservice();

                //////////////////////////////////////////////////////////////////////

                sql_database.LoadDatabaseMetadata(_DatabaseName, _Conn);

                // check for loading errors
                //foreach (var item in sql_database.ErrorList)
                //    DisplayMessage(item.Message, true);

                #region Basic options
                if (clbOutputOptions.CheckedItems.Contains(Properties.Resource.OptMiscRemoveExistingScripts))
                    DeleteOldScriptFiles();

                if (clbOutputOptions.CheckedItems.Contains(Properties.Resource.OptSQLCreateHelperSp))
                {
                    string script = codegeneratorSql.GenerateGetTableDetailsStoredProcedure();
                    file_name = Combine(_OutputPath, ConfigurationManager.AppSettings["DefaultTableDetailsSPName"]);

                    FileIo.WriteToFile(file_name, script);
                }

                //if (clbOutputOptions.CheckedItems.Contains(Properties.Resource.OptAspCreateMasterPage))
                //{
                //    // grab a list of all the selected tables
                //    var selected_tables = new List<string>();

                //    foreach (string table_name in clbReactSqlTables.CheckedItems)
                //        selected_tables.Add(table_name);

                //    output = codegeneratorAsp.GenerateMasterPageCodeInFront(sql_database.Name, selected_tables);
                //    file_name = Combine(_OutputPath, _DirectoryReactPages, output.Name);

                //    FileIo.WriteToFile(file_name, output.Body);

                //    output = codegeneratorAsp.GenerateMasterPageCodeBehind(sql_database.Name);
                //    file_name = Combine(_OutputPath, _DirectoryReactPages, output.Name);

                //    FileIo.WriteToFile(file_name, output.Body);
                //}

                //if (clbOutputOptions.CheckedItems.Contains(Properties.Resource.OptAspCreateCSSPage))
                //{
                //    output = codegeneratorAsp.GenerateCSSSheet(_DatabaseName);

                //    // this file is a css file
                //    output.Name += ".css";

                //    file_name = Combine(_OutputPath, _DirectoryReactPages, output.Name);

                //    FileIo.WriteToFile(file_name, output.Body);
                //}

                //if (clbOutputOptions.CheckedItems.Contains(Properties.Resource.OptAspCreateWebConfig))
                //{
                //    output = codegeneratorAsp.GenerateWebConfig(_Conn);
                //    file_name = Combine(_OutputPath, _DirectoryReactPages, output.Name);

                //    FileIo.WriteToFile(file_name, output.Body);
                //}

                //if (clbOutputOptions.CheckedItems.Contains(Properties.Resource.OptAspCreateDefaultPage))
                //{
                //    output = codegeneratorAsp.GenerateDefaultPageCodeInFront(sql_database.Name);
                //    file_name = Combine(_OutputPath, _DirectoryReactPages, output.Name);

                //    FileIo.WriteToFile(file_name, output.Body);

                //    output = codegeneratorAsp.GenerateDefaultPageCodeBehind(sql_database.Name);

                //    FileIo.WriteToFile(file_name, output.Body);
                //}
                #endregion

                #region Sql Code Generation
                if (clbTsqlSqlTables.CheckedItems.Count > 0 && clbTsqlSqlObjects.CheckedItems.Count > 0)
                {
                    sb.Append(codegeneratorSql.GenerateSqlScriptHeader(_DatabaseName));

                    foreach (string table_name in clbTsqlSqlTables.CheckedItems)
                    {
                        SqlTable sql_table = sql_database.Tables[table_name];

                        var sort_fields = new List<string>();
                        var search_fields = new List<string>();
                        var select_fields = new List<string>();

                        // get sort fields
                        if (clbTsqlSqlObjects.CheckedItems.Contains(Properties.Resource.SqlSelMany) ||
                            clbTsqlSqlObjects.CheckedItems.Contains(Properties.Resource.SqlSelManyByX) ||
                            clbTsqlSqlObjects.CheckedItems.Contains(Properties.Resource.SqlSelAll) ||
                            clbTsqlSqlObjects.CheckedItems.Contains(Properties.Resource.SqlSearchPaged))
                        {
                            var field_selector = new FieldSelector(sort_fields, sql_table, string.Format(Properties.Resource.SelectSortField, sql_table.Name));
                            field_selector.ShowDialog(this);
                            field_selector.Dispose();
                        }

                        // get search fields
                        if (clbTsqlSqlObjects.CheckedItems.Contains(Properties.Resource.SqlSearchPaged))
                        {
                            var field_selector = new FieldSelector(search_fields, sql_table, string.Format(Properties.Resource.SelectSearchField, sql_table.Name));
                            field_selector.ShowDialog(this);
                            field_selector.Dispose();
                        }

                        // get select fields
                        if (clbTsqlSqlObjects.CheckedItems.Contains(Properties.Resource.SqlSelManyByX) ||
                            clbTsqlSqlObjects.CheckedItems.Contains(Properties.Resource.SqlSelMany) ||
                            clbTsqlSqlObjects.CheckedItems.Contains(Properties.Resource.SqlDelMany))
                        {
                            var field_selector = new FieldSelector(select_fields, sql_table, string.Format(Properties.Resource.SelectSelectField, sql_table.Name));
                            field_selector.ShowDialog(this);
                            field_selector.Dispose();
                        }

                        // We are placeing all the SQl scripts in a temp dictionary
                        // object so we dont have to write them to a file untill they
                        // are all completely built.
                        var sql_procedures = new List<OutputObject>();

                        // should we be generating permissisons with the sp?
                        bool create_sql_permissions = clbOutputOptions.CheckedItems.Contains(Properties.Resource.OptSQLCreateSqlSpPerms);

                        // todo - wire up filter disabled flag? is this a useful feature?
                        // hard coded to false for now....

                        if (clbTsqlSqlObjects.CheckedItems.Contains(Properties.Resource.SqlSelSingle))
                        {
                            // temp hack - trying out new subscription model

                            var manifest = new GeneratorManifest
                            {
                                SearchFields = search_fields,
                                SelectFields = select_fields,
                                SortFields = sort_fields,
                                ConnectionString = _Conn,
                            };

                            manifest.SetOptionState(CodeGeneratorSql.INCLUDE_DISABLED_CHECK, GeneratorManifest.OptionsState.False);
                            manifest.SetOptionState(CodeGeneratorSql.GENERATE_STORED_PROC_PERMS, (create_sql_permissions == true) ? GeneratorManifest.OptionsState.True : GeneratorManifest.OptionsState.False);

                            // other settings?

                            sql_procedures.Add(codegeneratorSql.GenerateSelectSingleProc(sql_table, manifest));
                        }

                        if (clbTsqlSqlObjects.CheckedItems.Contains(Properties.Resource.SqlSelMany))
                            sql_procedures.Add(codegeneratorSql.GenerateSelectManyProc(sql_table, sort_fields, select_fields, create_sql_permissions, false));

                        if (clbTsqlSqlObjects.CheckedItems.Contains(Properties.Resource.SqlSelManyByX))
                            sql_procedures.Add(codegeneratorSql.GenerateSelectManyByXProc(sql_table, sort_fields, select_fields, create_sql_permissions, false));

                        if (clbTsqlSqlObjects.CheckedItems.Contains(Properties.Resource.SqlSelAll))
                            sql_procedures.Add(codegeneratorSql.GenerateSelectAllProc(sql_table, sort_fields, create_sql_permissions, false));

                        if (clbTsqlSqlObjects.CheckedItems.Contains(Properties.Resource.SqlSearchPaged))
                            sql_procedures.Add(codegeneratorSql.GeneratePaginatedSearchProc(sql_table, sort_fields, search_fields, create_sql_permissions, false));

                        if (clbTsqlSqlObjects.CheckedItems.Contains(Properties.Resource.SqlUpd))
                            sql_procedures.Add(codegeneratorSql.GenerateUpdateProc(sql_table, create_sql_permissions));

                        if (clbTsqlSqlObjects.CheckedItems.Contains(Properties.Resource.SqlIns))
                            sql_procedures.Add(codegeneratorSql.GenerateInsertProc(sql_table, create_sql_permissions));

                        if (clbTsqlSqlObjects.CheckedItems.Contains(Properties.Resource.SqlUpdIns))
                            sql_procedures.Add(codegeneratorSql.GenerateSetProc(sql_table, create_sql_permissions));

                        if (clbTsqlSqlObjects.CheckedItems.Contains(Properties.Resource.SqlDelSingle))
                            sql_procedures.Add(codegeneratorSql.GenerateDeleteSingleProc(sql_table, create_sql_permissions));

                        if (clbTsqlSqlObjects.CheckedItems.Contains(Properties.Resource.SqlDelMany))
                            sql_procedures.Add(codegeneratorSql.GenerateDeleteManyProc(sql_table, select_fields, create_sql_permissions));

                        if (clbTsqlSqlObjects.CheckedItems.Contains(Properties.Resource.SqlDelAll))
                            sql_procedures.Add(codegeneratorSql.GenerateDeleteAllProc(sql_table, create_sql_permissions));

                        // build one script or many?
                        if (clbOutputOptions.CheckedItems.Contains(Properties.Resource.OptSQLSeperateFiles))
                        {
                            string sql_header = sb.ToString();

                            foreach (var item in sql_procedures)
                            {
                                file_name = Combine(_OutputPath, _DirectorySql, item.Name + ConfigurationManager.AppSettings["DefaultSQLScriptFilename"]);
                                FileIo.WriteToFile(file_name, sql_header + item.Body);
                            }
                        }
                        else
                        {
                            foreach (var item in sql_procedures)
                                sb.Append(item.Body);

                            file_name = Combine(_OutputPath, _DirectorySql, _DatabaseName + ConfigurationManager.AppSettings["DefaultSQLScriptFilename"]);
                            FileIo.WriteToFile(file_name, sb.ToString());
                        }
                    }

                    DisplayMessage("SQL objects created.", false);
                }
                #endregion

                #region C# Code Generation
                if (clbCsharpSqlTables.CheckedItems.Count > 0 && clbCsharpObjects.CheckedItems.Count > 0)
                {
                    // todo: check for local flags being set, such as
                    // if (clbCsharpObjects.CheckedItems.Contains(Properties.Resource.OptCsharpIncludeBaseClassRefrence))

                    foreach (string table_name in clbCsharpSqlTables.CheckedItems)
                    {
                        if (!sql_database.Tables.ContainsKey(table_name))
                            continue;

                        SqlTable current_table = sql_database.Tables[table_name];

                        var data = new AutogenerationData()
                        {
                            NamespaceIncludes = _NamespaceIncludes,
                            Options = new Dictionary<string, bool>()
                            {
                                { CodeGeneratorCSharp.CONVERT_NULLABLE_FIELDS, clbCsharpObjects.CheckedItems.Contains(Properties.Resource.OptCsharpIncludeBaseClassRefrence) },
                            },
                            SqlTable = current_table,
                        };

                        // roll through each checked item 
                        foreach (var item in clbCsharpObjects.CheckedItems)
                        {
                            string methodName = codegeneratorCsharp.Methods[(string)item];

                            MethodInfo mi = codegeneratorCsharp.GetType().GetMethod($"{methodName}");

                            if (mi == null)
                                DisplayMessage($"unable to map method {methodName}", true);

                            output = (OutputObject)mi.Invoke(codegeneratorCsharp, new object[] { data });
                            file_name = Combine(_OutputPath, _DirectoryOrm, output.Name);
                            FileIo.WriteToFile(file_name, output.Body);
                        }

                        if (clbCsharpObjects.CheckedItems.Contains(Properties.Resource.CsharpEnum))
                        {
                            var key_fields = new List<string>();
                            var value_fields = new List<string>();

                            FieldSelector field_selector;

                            field_selector = new FieldSelector(key_fields, current_table, Properties.Resource.SelectEnumNameField, 1, 1);
                            field_selector.ShowDialog(this);
                            field_selector.Dispose();

                            field_selector = new FieldSelector(value_fields, current_table, Properties.Resource.SelectEnumValueField, 1, 1);
                            field_selector.ShowDialog(this);
                            field_selector.Dispose();

                            // Make sure that table has correct format
                            if (key_fields.Count != 1 || value_fields.Count != 1)
                            {
                                DisplayMessage("Cannot generate enum for " + table_name + ".", true);
                            }
                            else
                            {
                                output = codegeneratorCsharp.GenerateCSharpEnumeration(current_table, key_fields[0], value_fields[0], _Conn);
                                file_name = Combine(_OutputPath, _DirectoryEnums, output.Name);
                                FileIo.WriteToFile(file_name, output.Body);
                            }
                        }
                    }

                    // todo: do we want to write out the base class generation to the DAL dir?
                    if (clbCsharpObjects.CheckedItems.Contains(Properties.Resource.CsharpCreateBaseClass))
                    {
                        output = codegeneratorCsharp.GenerateCSharpBaseClass(_DatabaseName);
                        file_name = Combine(_OutputPath, _DirectoryDal, output.Name);

                        FileIo.WriteToFile(file_name, output.Body);
                    }

                    DisplayMessage("C# objects created", false);
                }
                #endregion

                #region WebService Code Generation
                if (clbWebServiceSqlTables.CheckedItems.Count > 0 && clbWebServiceObjects.CheckedItems.Count > 0)
                {
                    bool generate_base_controller = clbWebServiceObjects.CheckedItems.Contains(Properties.Resource.WebServiceControllerBase);
                    bool generate_response_base = clbWebServiceObjects.CheckedItems.Contains(Properties.Resource.WebServiceResponseObject);

                    codegeneratorRest.Options[CodeGeneratorWebservice.GENERATE_BASE_CONTROLLER] = generate_base_controller;
                    codegeneratorRest.Options[CodeGeneratorWebservice.GENERATE_BASE_CONTROLLER] = generate_response_base;
                    codegeneratorRest.Options[CodeGeneratorWebservice.INCLUDE_SESSION_TOKEN] = true; // todo, wire up?

                    foreach (string table_name in clbWebServiceSqlTables.CheckedItems)
                    {
                        if (!sql_database.Tables.ContainsKey(table_name))
                            continue;

                        SqlTable current_table = sql_database.Tables[table_name];

                        if (clbWebServiceObjects.CheckedItems.Contains(Properties.Resource.WebServiceController))
                        {
                            output = codegeneratorRest.GenerateWebServiceControllerClass(current_table, _NamespaceIncludes, codegeneratorRest.Options);
                            file_name = Combine(_OutputPath, _DirectoryWebService, output.Name);
                            FileIo.WriteToFile(file_name, output.Body);
                        }
                    }

                    if (clbWebServiceObjects.CheckedItems.Contains(Properties.Resource.WebServiceControllerBase))
                    {
                        output = codegeneratorRest.GenerateWebServiceBaseControllerClass();
                        file_name = Combine(_OutputPath, _DirectoryWebService, output.Name);
                        FileIo.WriteToFile(file_name, output.Body);
                    }
                    if (clbWebServiceObjects.CheckedItems.Contains(Properties.Resource.WebServiceResponseObject))
                    {
                        output = codegeneratorRest.GenerateResponseBaseClass();
                        file_name = Combine(_OutputPath, _DirectoryWebService, output.Name);
                        FileIo.WriteToFile(file_name, output.Body);
                    }
                    if (clbWebServiceObjects.CheckedItems.Contains(Properties.Resource.WebServicePagingObject))
                    {
                        output = codegeneratorRest.GeneratePagingClass();
                        file_name = Combine(_OutputPath, _DirectoryWebService, output.Name);
                        FileIo.WriteToFile(file_name, output.Body);
                    }

                    DisplayMessage("Webservice objects created", false);
                }

                #endregion

                #region React Code Generation
                if (clbReactSqlTables.CheckedItems.Count > 0 && clbReactObjects.CheckedItems.Count > 0)
                {
                    foreach (string table_name in clbReactSqlTables.CheckedItems)
                    {
                        SqlTable current_table = sql_database.Tables[table_name];

                        //if (clbReactObjects.CheckedItems.Contains(Properties.Resource.ReactCreateEditPage))
                        //{
                        //    output = codegeneratorAsp.GenerateWebEditPageCodeInFront(current_table, clbCsharpObjects.CheckedItems.Contains(Properties.Resource.OptAspCreatePageAsConrol));
                        //    file_name = Combine(_OutputPath, _DirectoryReactPages, output.Name);
                        //    FileIo.WriteToFile(file_name, output.Body);

                        //    output = codegeneratorAsp.GenerateWebEditPageCodeBehind(current_table, _NamespaceIncludes);
                        //    file_name = Combine(_OutputPath, _DirectoryReactPages, output.Name);
                        //    FileIo.WriteToFile(file_name, output.Body);
                        //}

                        //if (clbReactObjects.CheckedItems.Contains(Properties.Resource.ReactCreateListPage))
                        //{
                        //    output = codegeneratorAsp.GenerateWebListPageCodeInFront(current_table, clbCsharpObjects.CheckedItems.Contains(Properties.Resource.OptAspCreatePageAsConrol));
                        //    file_name = Combine(_OutputPath, _DirectoryReactPages, output.Name);
                        //    FileIo.WriteToFile(file_name, output.Body);

                        //    output = codegeneratorAsp.GenerateWebListPageCodeBehind(current_table, _NamespaceIncludes);
                        //    file_name = Combine(_OutputPath, _DirectoryReactPages, output.Name);
                        //    FileIo.WriteToFile(file_name, output.Body);
                        //}

                        //if (clbReactObjects.CheckedItems.Contains(Properties.Resource.ReactCreateViewPage))
                        //{
                        //    output = codegeneratorAsp.GenerateWebViewPageCodeInFront(current_table);
                        //    file_name = Combine(_OutputPath, _DirectoryReactPages, output.Name);
                        //    FileIo.WriteToFile(file_name, output.Body);
                        //}
                    }

                    DisplayMessage("React objects created.", false);
                }
                #endregion

                #region Data Export Generation
                if (clbExportSqlTables.CheckedItems.Count > 0 && clbExportOptionsTables.CheckedItems.Count > 0)
                {
                    bool generate_with_attributes = clbOutputOptions.CheckedItems.Contains(Properties.Resource.OptXmlFormat);

                    string name_space = string.Empty;

                    if (clbOutputOptions.CheckedItems.Contains(Properties.Resource.OptXmlIncludeNs))
                        name_space = _DatabaseName;

                    foreach (string table_name in clbExportSqlTables.CheckedItems)
                    {
                        if (!sql_database.Tables.ContainsKey(table_name))
                            continue;

                        SqlTable current_table = sql_database.Tables[table_name];

                        // Json Export
                        if (clbExportOptionsTables.CheckedItems.Contains(Properties.Resource.ExportJsonData))
                        {
                            DataTable data_table = codegeneratorCsharp.GenerateSqlTableData(current_table, _Conn);

                            if (generate_with_attributes)
                            {
                                foreach (DataColumn column in data_table.Columns)
                                    column.ColumnMapping = MappingType.Attribute;
                            }

                            data_table.TableName = table_name;
                            string json_output = JsonConverter.DataSetToJsonString(data_table);
                            file_name = Combine(_OutputPath, _DirectoryExportData, table_name + ConfigurationManager.AppSettings["ExportJsonSuffix"]);

                            FileIo.WriteToFile(file_name, json_output);
                        }

                        if (clbExportOptionsTables.CheckedItems.Contains(Properties.Resource.ExportXmlData))
                        {
                            DataTable data_table = codegeneratorCsharp.GenerateSqlTableData(current_table, _Conn);

                            if (generate_with_attributes)
                            {
                                foreach (DataColumn column in data_table.Columns)
                                    column.ColumnMapping = MappingType.Attribute;
                            }

                            data_table.TableName = table_name;
                            string xml_output = XmlConverter.DataTableToXmlString(data_table, name_space);
                            file_name = Combine(_OutputPath, _DirectoryExportData, table_name + ConfigurationManager.AppSettings["ExportXmlSuffix"]);

                            FileIo.WriteToFile(file_name, xml_output);
                        }

                        // Xml Loader object
                        if (clbExportOptionsTables.CheckedItems.Contains(Properties.Resource.ImportJsonObject))
                        {
                            output = codegeneratorCsharp.GenerateXmlLoader(current_table);
                            file_name = Combine(_OutputPath, _DirectoryExportData, output.Name);
                            FileIo.WriteToFile(file_name, output.Body);
                        }
                    }

                    DisplayMessage("XML objects created.", false);
                }
                #endregion
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
                System.Media.SystemSounds.Exclamation.Play();
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            try
            {
                // 1) Connect to Db
                // 2) enable tabs that are now valid
                // 3) clear out db list and populate with new data

                var db = new Database(_Conn);

                List<string> processer(SqlDataReader reader)
                {
                    var output = new List<string>();

                    while (reader.Read())
                    {
                        string buffer = (string)reader["DATABASE_NAME"];

                        if (!_FilteredTableNames.Contains(buffer))
                            output.Add(buffer);
                    }

                    return output;
                }

                var results = db.ExecuteQuery("[Master].[dbo].[sp_databases]", null, processer);

                if (results == null)
                {
                    // connection failed, bail
                    DisplayMessage("Failed to establish a connection to database. Please check your connection information.", true);
                    return;
                }

                string connection_message = string.Empty;

                if (ckbLocalDb.Checked == true)
                    connection_message = Properties.Resource.ConnectionMessage + "(localhost)";
                else
                    connection_message = Properties.Resource.ConnectionMessage + txtServerName.Text;

                SetConnectStatusLabel(connection_message);
                DisplayMessage(connection_message, false);

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
                SetConnectStatusLabel(Properties.Resource.ConnectionMessage + "(No Connection)");
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
                    ResetSqlTab();
                    break;

                case "tabCsharp":
                    ResetCsharpTab();
                    break;

                case "tabAsp":
                    ResetReactTab();
                    break;

                case "tabWebService":
                    ResetWebServiceTab();
                    break;

                case "tabExport":
                    ResetExportTab();
                    break;

                case "tabOutput":
                    ResetOutputTab();
                    break;

                case "tabAbout":
                    break;

                default:
                    throw new Exception($"Unknown tab name '{tabcontrolAutoCodeGen.SelectedTab.Name}'");
            }

            ValidateTab();
        }

        private void btnSetDirectory_Click(object sender, EventArgs e)
        {
            string output_directory = Directory.GetCurrentDirectory();
            txtOutputPath.Text = output_directory;
            DisplayMessage("Output directory set to " + output_directory, false);
        }

        private void btnCheckRegex_Click(object sender, EventArgs e)
        {
            string output_data = string.Empty;

            try
            {
                if (txtTableNameRegex.Text != string.Empty)
                {
                    if (clbTsqlSqlTables.Items.Count > 0)
                    {
                        string regex_string = txtTableNameRegex.Text;
                        string test_string;

                        // we need to grab a table name to try the regex on.
                        // try to get a checked item from the Csharp list, or
                        // anything from the SQL table list.
                        if (clbCsharpSqlTables.CheckedItems.Count != 0)
                        {
                            test_string = (string)clbCsharpSqlTables.CheckedItems[0];
                        }
                        else
                        {
                            test_string = (string)clbTsqlSqlTables.Items[0];
                        }

                        output_data = Regex.Replace(test_string, txtTableNameRegex.Text, string.Empty, RegexOptions.IgnoreCase);
                    }
                    else
                    {
                        DisplayMessage("Cannot evaluate regular expression, no table names are available to parse.", true);
                    }
                }
            }
            catch
            {
                DisplayMessage("The regular expression you entered does not parse.", true);
            }

            lblRegexResult.Text = "Output: " + output_data;
        }

        private void btnAddNamespace_Click(object sender, EventArgs e)
        {
            string new_namespace = this.cboNamespaceIncludes.Text;

            if (!_NamespaceIncludes.Contains(new_namespace))
            {
                _NamespaceIncludes.Add(new_namespace);
                UpdateComboBoxList(this.cboNamespaceIncludes, _NamespaceIncludes);
            }

            cboNamespaceIncludes.Text = string.Empty;
        }

        private void btnOpenOutputDirectory_Click(object sender, EventArgs e)
        {
            OpenFolder(txtOutputPath.Text);
        }

        // select/deselect all code
        private void btnToggleTsqlSqlTables_Click(object sender, EventArgs e)
        {
            // SQL table
            if (btnToggleTsqlSqlTables.Text == Properties.Resource.SelectAll)
            {
                btnToggleTsqlSqlTables.Text = Properties.Resource.DeselectAll;
                ToggleCheckBoxes(clbTsqlSqlTables, true);
            }
            else
            {
                btnToggleTsqlSqlTables.Text = Properties.Resource.SelectAll;
                ToggleCheckBoxes(clbTsqlSqlTables, false);
            }
        }

        private void btnToggleTsqlSqlObjects_Click(object sender, EventArgs e)
        {
            // SQL object
            if (btnToggleTsqlSqlObjects.Text == Properties.Resource.SelectAll)
            {
                btnToggleTsqlSqlObjects.Text = Properties.Resource.DeselectAll;
                ToggleCheckBoxes(clbTsqlSqlObjects, true);
            }
            else
            {
                btnToggleTsqlSqlObjects.Text = Properties.Resource.SelectAll;
                ToggleCheckBoxes(clbTsqlSqlObjects, false);
            }
        }

        private void btnToggleCsharpSqlTables_Click(object sender, EventArgs e)
        {
            // C# table
            if (btnToggleCsharpSqlTables.Text == Properties.Resource.SelectAll)
            {
                btnToggleCsharpSqlTables.Text = Properties.Resource.DeselectAll;
                ToggleCheckBoxes(clbCsharpSqlTables, true);
            }
            else
            {
                btnToggleCsharpSqlTables.Text = Properties.Resource.SelectAll;
                ToggleCheckBoxes(clbCsharpSqlTables, false);
            }
        }

        private void btnToggleCSharpObjects_Click(object sender, EventArgs e)
        {
            if (btnToggleCSharpObjects.Text == Properties.Resource.SelectAll)
            {
                btnToggleCSharpObjects.Text = Properties.Resource.DeselectAll;
                ToggleCheckBoxes(clbCsharpObjects, true);
            }
            else
            {
                btnToggleCSharpObjects.Text = Properties.Resource.SelectAll;
                ToggleCheckBoxes(clbCsharpObjects, false);
            }
        }

        private void btnToggleWebServicesSqlTables_Click(object sender, EventArgs e)
        {
            if (btnToggleWebServicesSqlTables.Text == Properties.Resource.SelectAll)
            {
                btnToggleWebServicesSqlTables.Text = Properties.Resource.DeselectAll;
                ToggleCheckBoxes(clbWebServiceSqlTables, true);
            }
            else
            {
                btnToggleWebServicesSqlTables.Text = Properties.Resource.SelectAll;
                ToggleCheckBoxes(clbWebServiceSqlTables, false);
            }
        }

        private void btnToggleWebServiceObjects_Click(object sender, EventArgs e)
        {
            if (btnToggleWebServiceObjects.Text == Properties.Resource.SelectAll)
            {
                btnToggleWebServiceObjects.Text = Properties.Resource.DeselectAll;
                ToggleCheckBoxes(clbWebServiceObjects, true);
            }
            else
            {
                btnToggleWebServiceObjects.Text = Properties.Resource.SelectAll;
                ToggleCheckBoxes(clbWebServiceObjects, false);
            }
        }

        private void btnToggleAspSqlTables_Click(object sender, EventArgs e)
        {
            // ASP table
            if (btnToggleReactSqlTables.Text == Properties.Resource.SelectAll)
            {
                btnToggleReactSqlTables.Text = Properties.Resource.DeselectAll;
                ToggleCheckBoxes(clbReactSqlTables, true);
            }
            else
            {
                btnToggleReactSqlTables.Text = Properties.Resource.SelectAll;
                ToggleCheckBoxes(clbReactSqlTables, false);
            }
        }

        private void btnToggleASPObjects_Click(object sender, EventArgs e)
        {
            if (btnToggleASPObjects.Text == Properties.Resource.SelectAll)
            {
                btnToggleASPObjects.Text = Properties.Resource.DeselectAll;
                ToggleCheckBoxes(clbReactObjects, true);
            }
            else
            {
                btnToggleASPObjects.Text = Properties.Resource.SelectAll;
                ToggleCheckBoxes(clbReactObjects, false);
            }
        }

        private void btnToggleWebXmlSqlTables_Click(object sender, EventArgs e)
        {
            // XML table
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

        private void btnToggleWebXmlObjects_Click(object sender, EventArgs e)
        {
            if (btnToggleExportObjects.Text == Properties.Resource.SelectAll)
            {
                btnToggleExportObjects.Text = Properties.Resource.DeselectAll;
                ToggleCheckBoxes(clbExportOptionsTables, true);
            }
            else
            {
                btnToggleExportObjects.Text = Properties.Resource.SelectAll;
                ToggleCheckBoxes(clbExportOptionsTables, false);
            }
        }

        private void btnToggleOutput_Click(object sender, EventArgs e)
        {
            if (btnToggleOutput.Text == Properties.Resource.SelectAll)
            {
                btnToggleOutput.Text = Properties.Resource.DeselectAll;
                ToggleCheckBoxes(clbOutputOptions, true);
            }
            else
            {
                btnToggleOutput.Text = Properties.Resource.SelectAll;
                ToggleCheckBoxes(clbOutputOptions, false);
            }
        }

        // Other events
        private void cmbDatabaseList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            try
            {
                GetDbTables();

                // reset all tabs that have table specific data
                ResetSqlTab();
                ResetCsharpTab();
                ResetWebServiceTab();
                ResetReactTab();
                ResetExportTab();

                ValidateTab();
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

                btnGenerateCode.Enabled = !string.IsNullOrEmpty(cmbDatabaseList.Text);
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

        private void cboNamespaceIncludes_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Delete:

                    _NamespaceIncludes.Remove(cboNamespaceIncludes.Text);
                    this.cboNamespaceIncludes.Text = string.Empty;
                    UpdateComboBoxList(this.cboNamespaceIncludes, _NamespaceIncludes);
                    break;
            }
        }

        private string Combine(string path1, string path2, string path3 = null, string path4 = null, string path5 = null)
        {
            // todo remove

            var sb = new StringBuilder();

            sb.Append(path1);

            if (path1.LastIndexOf('\\') != (path1.Length - 1) && path2.IndexOf('\\') != 0)
                sb.Append('\\');

            sb.Append(path2);

            if (path3 != null)
            {
                if (path2.LastIndexOf('\\') != (path2.Length - 1) && path3.IndexOf('\\') != 0)
                    sb.Append('\\');

                sb.Append(path3);
            }

            if (path4 != null)
            {
                if (path3.LastIndexOf('\\') != (path3.Length - 1) && path4.IndexOf('\\') != 0)
                    sb.Append('\\');

                sb.Append(path4);
            }

            if (path5 != null)
            {
                if (path4.LastIndexOf('\\') != (path4.Length - 1) && path5.IndexOf('\\') != 0)
                    sb.Append('\\');

                sb.Append(path5);
            }

            // in the event we got some doubled characters, remove them
            sb = sb.Replace("\\\\", "\\");

            return sb.ToString();
        }
    }
}