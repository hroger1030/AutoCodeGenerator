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
using DAL.Net;
using DAL.Net.SqlMetadata;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoCodeGen
{
    [SupportedOSPlatform("windows")]
    public partial class Main : Form
    {
        private const string APP_NAME = "Jolly Roger Code Generator";
        private const string PRODUCT_VERSION = "3.0";
        private const string GITHUB_URL = "https://github.com/hroger1030/AutoCodeGenerator";
        private const string DEFAULT_SQL_CONN_STRING = "Server=(localdb)\\MSSQLLocalDB;Database=master;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;Connect Timeout=2;";
        private const string OUTPUT_DIRECTORY_NAME = "\\GeneratedOutput";

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

        private AesEncryption _AesEncryption;
        private List<TableMetadata> _DbTables;
        private Dictionary<string, IGenerator> _Generators;

        // Counts to help manage onChecked events for checkbox lists.
        // When event is fired, change has not been applied to object 
        // yet, so extra variables are required to track state.

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

                if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.ConnectionString))
                    this.txtConn.Text = _AesEncryption.Decrypt(Properties.Settings.Default.ConnectionString, PASS_PHRASE, SALT);

                // set form size from saved settings
                this.Size = new Size(Properties.Settings.Default.MainFormWidth, Properties.Settings.Default.MainFormHeight);

                _Generators = LoadPlugins();

                txtConn.Text = Properties.Settings.Default.ConnectionString;
                DisplayMessage($"{APP_NAME}, Version {PRODUCT_VERSION}", false);
                DisplayMessage($"Released under MIT OSS license, source code available at {GITHUB_URL}", false);
            }
            catch (Exception ex)
            {
                DisplayMessage(ex.Message, true);
            }
        }

        // events

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(this.txtConn.Text))
                Properties.Settings.Default.ConnectionString = _AesEncryption.Encrypt(this.txtConn.Text, PASS_PHRASE, SALT);

            Properties.Settings.Default.OutputPath = txtOutputPath.Text;
            Properties.Settings.Default.MainFormHeight = this.Size.Height;
            Properties.Settings.Default.MainFormWidth = this.Size.Width;

            Properties.Settings.Default.Save();
        }

        private void btnGenerateCode_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            try
            {
                if (!Directory.Exists(txtOutputPath.Text))
                {
                    DisplayMessage("Specified output directory does not exist, halting code generation.", true);
                    return;
                }

                var sqlDatabase = new SqlDatabase();
                sqlDatabase.LoadDatabaseMetadata(cmbDatabaseList.Text, txtConn.Text);

                var output = new List<OutputObject>();

                // get list of selected items here

                foreach (var item in output)
                {
                    var fileName = Path.Combine(txtOutputPath.Text, item.OutputPath, item.FileName);
                    FileIo.WriteToFile(fileName, item.Body);
                }

                DisplayMessage("Objects created.", false);
            }
            catch (Exception ex)
            {
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
            if (string.IsNullOrEmpty(txtConn.Text))
                return;

            Cursor = Cursors.WaitCursor;

            // 1) Connect to Db
            // 2) enable tabs that are now valid
            // 3) clear out db list and populate with new data

            try
            {
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

                var results = await db.ExecuteQueryAsync("[Master].[dbo].[sp_databases]", null, processer);

                if (results == null)
                {
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
                DisplayMessage(ex.Message, true);
                DisplayMessage("Failed to connect to server.");
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnSetDirectory_Click(object sender, EventArgs e)
        {
            string outputDirectory = Directory.GetCurrentDirectory() + OUTPUT_DIRECTORY_NAME;

            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            txtOutputPath.Text = outputDirectory;
            DisplayMessage("Output directory set to " + outputDirectory, false);
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

        private void btnUseDefaultConn_Click(object sender, EventArgs e)
        {
            txtConn.Text = DEFAULT_SQL_CONN_STRING;
        }

        private void btnCleanOutput_Click(object sender, EventArgs e)
        {
            var outputPath = txtOutputPath.Text;

            try
            {
                //string applicationPath = Path.GetDirectoryName(Application.ExecutablePath);

                if (outputPath == null || !Directory.Exists(outputPath))
                    return;

                var fileList = Directory.GetFiles(outputPath, "*.*", SearchOption.AllDirectories);

                foreach (string filename in fileList)
                    File.Delete(filename);

                var directoryList = Directory.GetDirectories(outputPath, "*.*", SearchOption.TopDirectoryOnly);

                foreach (string directoryName in directoryList)
                    Directory.Delete(directoryName, false);
            }
            catch
            {
                DisplayMessage($"Error encountered in removing script files from {outputPath}. Please make sure they aren't open in other programs.", true);
                return;
            }

            DisplayMessage($"Script files removed from {outputPath}", false);
        }

        private async void cmbDatabaseList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            try
            {
                await GetDbTables();

                // reset all tabs that have table specific data

            }
            catch (Exception ex)
            {
                DisplayMessage(ex.Message, true);
            }
            finally
            {
                DisplayMessage("Changed to database " + cmbDatabaseList.Text, false);
                Cursor = Cursors.Default;
            }
        }

        // helper functions

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
        /// function takes the name of the current Db that we are working with,
        /// and populates the _DbTablesList with the names of the tables in that
        /// database. NOTE that data tables list is not currently used for anything. 
        /// </summary>
        private async Task GetDbTables()
        {
            var databaseName = cmbDatabaseList.Text;

            if (string.IsNullOrEmpty(databaseName))
            {
                DisplayMessage("Database name is null or empty, cannot load table data", true);
                return;
            }

            var db = new Database(txtConn.Text);

            string sql_query = $"[{databaseName}].[dbo].[sp_tables] null,null,null,\"'TABLE'\"";

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

        /// <summary>
        /// Scans the assembly for all available plugins that implement IPlugin interface.
        /// returns a dictionary of plugin names and plugin instances.
        /// </summary>
        private Dictionary<string, IGenerator> LoadPlugins()
        {
            var pluginType = typeof(IGenerator);
            var assembly = typeof(IGenerator).Assembly;

            var buffer = assembly
                .GetTypes()
                .Where(t =>
                    t.IsClass &&
                    !t.IsAbstract &&
                    pluginType.IsAssignableFrom(t) &&
                    t.Namespace != null &&
                    t.Namespace.StartsWith("AutoCodeGenLibrary"))
                .Select(t => (IGenerator)Activator.CreateInstance(t)!);

            var output = new Dictionary<string, IGenerator>();

            foreach (var plugin in buffer)
            {
                output.Add(plugin.Name, plugin);
            }

            return output;
        }
    }
}