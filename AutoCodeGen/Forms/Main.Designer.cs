namespace AutoCodeGen
{
    partial class Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            btnGenerateCode = new System.Windows.Forms.Button();
            btnResetCurrentTab = new System.Windows.Forms.Button();
            tabAbout = new System.Windows.Forms.TabPage();
            lblCopyright = new System.Windows.Forms.Label();
            lnkEmail = new System.Windows.Forms.LinkLabel();
            lnkWebLink = new System.Windows.Forms.LinkLabel();
            pbxLogo = new System.Windows.Forms.PictureBox();
            lblAuthor = new System.Windows.Forms.Label();
            lblVersion = new System.Windows.Forms.Label();
            lblApplicationTitle = new System.Windows.Forms.Label();
            tabOutput = new System.Windows.Forms.TabPage();
            btnSetDirectory = new System.Windows.Forms.Button();
            btnToggleOutput = new System.Windows.Forms.Button();
            lblOutputPath = new System.Windows.Forms.Label();
            txtOutputPath = new System.Windows.Forms.TextBox();
            lblOutputOptions = new System.Windows.Forms.Label();
            clbOutputOptions = new System.Windows.Forms.CheckedListBox();
            tabExport = new System.Windows.Forms.TabPage();
            btnToggleExportObjects = new System.Windows.Forms.Button();
            lblXml = new System.Windows.Forms.Label();
            clbExportOptions = new System.Windows.Forms.CheckedListBox();
            btnToggleExportSqlTables = new System.Windows.Forms.Button();
            lblExportTables = new System.Windows.Forms.Label();
            clbExportSqlTables = new System.Windows.Forms.CheckedListBox();
            tabGenerators = new System.Windows.Forms.TabPage();
            btnToggleSql = new System.Windows.Forms.Button();
            lblGenerators = new System.Windows.Forms.Label();
            btnToggleGenerators = new System.Windows.Forms.Button();
            lblSQLTables = new System.Windows.Forms.Label();
            clbGenerators = new System.Windows.Forms.CheckedListBox();
            clbSqlTables = new System.Windows.Forms.CheckedListBox();
            tabServer = new System.Windows.Forms.TabPage();
            btnUseDefaultConn = new System.Windows.Forms.Button();
            txtConn = new System.Windows.Forms.TextBox();
            lblLogon = new System.Windows.Forms.Label();
            cmbDatabaseList = new System.Windows.Forms.ComboBox();
            lblDatabase = new System.Windows.Forms.Label();
            btnConnect = new System.Windows.Forms.Button();
            tabcontrolAutoCodeGen = new System.Windows.Forms.TabControl();
            lvMessaging = new System.Windows.Forms.ListView();
            Message = new System.Windows.Forms.ColumnHeader();
            lblMessages = new System.Windows.Forms.Label();
            btnOpenOutputDirectory = new System.Windows.Forms.Button();
            tabAbout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pbxLogo).BeginInit();
            tabOutput.SuspendLayout();
            tabExport.SuspendLayout();
            tabGenerators.SuspendLayout();
            tabServer.SuspendLayout();
            tabcontrolAutoCodeGen.SuspendLayout();
            SuspendLayout();
            // 
            // btnGenerateCode
            // 
            resources.ApplyResources(btnGenerateCode, "btnGenerateCode");
            btnGenerateCode.Name = "btnGenerateCode";
            btnGenerateCode.UseVisualStyleBackColor = true;
            btnGenerateCode.Click += btnGenerateCode_Click;
            // 
            // btnResetCurrentTab
            // 
            resources.ApplyResources(btnResetCurrentTab, "btnResetCurrentTab");
            btnResetCurrentTab.Name = "btnResetCurrentTab";
            btnResetCurrentTab.UseVisualStyleBackColor = true;
            btnResetCurrentTab.Click += btnResetCurrentTab_Click;
            // 
            // tabAbout
            // 
            tabAbout.Controls.Add(lblCopyright);
            tabAbout.Controls.Add(lnkEmail);
            tabAbout.Controls.Add(lnkWebLink);
            tabAbout.Controls.Add(pbxLogo);
            tabAbout.Controls.Add(lblAuthor);
            tabAbout.Controls.Add(lblVersion);
            tabAbout.Controls.Add(lblApplicationTitle);
            resources.ApplyResources(tabAbout, "tabAbout");
            tabAbout.Name = "tabAbout";
            tabAbout.UseVisualStyleBackColor = true;
            // 
            // lblCopyright
            // 
            resources.ApplyResources(lblCopyright, "lblCopyright");
            lblCopyright.Name = "lblCopyright";
            // 
            // lnkEmail
            // 
            resources.ApplyResources(lnkEmail, "lnkEmail");
            lnkEmail.Name = "lnkEmail";
            lnkEmail.TabStop = true;
            lnkEmail.LinkClicked += lnkEmail_LinkClicked;
            // 
            // lnkWebLink
            // 
            resources.ApplyResources(lnkWebLink, "lnkWebLink");
            lnkWebLink.Name = "lnkWebLink";
            lnkWebLink.TabStop = true;
            lnkWebLink.LinkClicked += lnkWebLink_LinkClicked;
            // 
            // pbxLogo
            // 
            resources.ApplyResources(pbxLogo, "pbxLogo");
            pbxLogo.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            pbxLogo.Name = "pbxLogo";
            pbxLogo.TabStop = false;
            pbxLogo.Tag = "";
            // 
            // lblAuthor
            // 
            resources.ApplyResources(lblAuthor, "lblAuthor");
            lblAuthor.Name = "lblAuthor";
            // 
            // lblVersion
            // 
            resources.ApplyResources(lblVersion, "lblVersion");
            lblVersion.Name = "lblVersion";
            // 
            // lblApplicationTitle
            // 
            resources.ApplyResources(lblApplicationTitle, "lblApplicationTitle");
            lblApplicationTitle.Name = "lblApplicationTitle";
            // 
            // tabOutput
            // 
            tabOutput.Controls.Add(btnSetDirectory);
            tabOutput.Controls.Add(btnToggleOutput);
            tabOutput.Controls.Add(lblOutputPath);
            tabOutput.Controls.Add(txtOutputPath);
            tabOutput.Controls.Add(lblOutputOptions);
            tabOutput.Controls.Add(clbOutputOptions);
            resources.ApplyResources(tabOutput, "tabOutput");
            tabOutput.Name = "tabOutput";
            tabOutput.UseVisualStyleBackColor = true;
            // 
            // btnSetDirectory
            // 
            resources.ApplyResources(btnSetDirectory, "btnSetDirectory");
            btnSetDirectory.Name = "btnSetDirectory";
            btnSetDirectory.UseVisualStyleBackColor = true;
            btnSetDirectory.Click += btnSetDirectory_Click;
            // 
            // btnToggleOutput
            // 
            resources.ApplyResources(btnToggleOutput, "btnToggleOutput");
            btnToggleOutput.Name = "btnToggleOutput";
            btnToggleOutput.UseVisualStyleBackColor = true;
            btnToggleOutput.Click += btnToggleOutput_Click;
            // 
            // lblOutputPath
            // 
            resources.ApplyResources(lblOutputPath, "lblOutputPath");
            lblOutputPath.Name = "lblOutputPath";
            // 
            // txtOutputPath
            // 
            resources.ApplyResources(txtOutputPath, "txtOutputPath");
            txtOutputPath.Name = "txtOutputPath";
            txtOutputPath.TextChanged += txtOutputPath_TextChanged;
            // 
            // lblOutputOptions
            // 
            resources.ApplyResources(lblOutputOptions, "lblOutputOptions");
            lblOutputOptions.Name = "lblOutputOptions";
            // 
            // clbOutputOptions
            // 
            resources.ApplyResources(clbOutputOptions, "clbOutputOptions");
            clbOutputOptions.CheckOnClick = true;
            clbOutputOptions.FormattingEnabled = true;
            clbOutputOptions.Name = "clbOutputOptions";
            clbOutputOptions.Sorted = true;
            clbOutputOptions.ItemCheck += clbOutputOptions_ItemCheck;
            // 
            // tabExport
            // 
            tabExport.Controls.Add(btnToggleExportObjects);
            tabExport.Controls.Add(lblXml);
            tabExport.Controls.Add(clbExportOptions);
            tabExport.Controls.Add(btnToggleExportSqlTables);
            tabExport.Controls.Add(lblExportTables);
            tabExport.Controls.Add(clbExportSqlTables);
            resources.ApplyResources(tabExport, "tabExport");
            tabExport.Name = "tabExport";
            tabExport.UseVisualStyleBackColor = true;
            // 
            // btnToggleExportObjects
            // 
            resources.ApplyResources(btnToggleExportObjects, "btnToggleExportObjects");
            btnToggleExportObjects.Name = "btnToggleExportObjects";
            btnToggleExportObjects.UseVisualStyleBackColor = true;
            btnToggleExportObjects.Click += btnToggleExportObjects_Click;
            // 
            // lblXml
            // 
            resources.ApplyResources(lblXml, "lblXml");
            lblXml.Name = "lblXml";
            // 
            // clbExportOptions
            // 
            resources.ApplyResources(clbExportOptions, "clbExportOptions");
            clbExportOptions.CheckOnClick = true;
            clbExportOptions.FormattingEnabled = true;
            clbExportOptions.Name = "clbExportOptions";
            clbExportOptions.Sorted = true;
            clbExportOptions.ItemCheck += clbExportOptionTables_ItemCheck;
            // 
            // btnToggleExportSqlTables
            // 
            resources.ApplyResources(btnToggleExportSqlTables, "btnToggleExportSqlTables");
            btnToggleExportSqlTables.Name = "btnToggleExportSqlTables";
            btnToggleExportSqlTables.UseVisualStyleBackColor = true;
            btnToggleExportSqlTables.Click += btnToggleExportSqlTables_Click;
            // 
            // lblExportTables
            // 
            resources.ApplyResources(lblExportTables, "lblExportTables");
            lblExportTables.Name = "lblExportTables";
            // 
            // clbExportSqlTables
            // 
            resources.ApplyResources(clbExportSqlTables, "clbExportSqlTables");
            clbExportSqlTables.CheckOnClick = true;
            clbExportSqlTables.FormattingEnabled = true;
            clbExportSqlTables.Name = "clbExportSqlTables";
            clbExportSqlTables.ItemCheck += clbExportSqlTables_ItemCheck;
            // 
            // tabGenerators
            // 
            tabGenerators.Controls.Add(btnToggleSql);
            tabGenerators.Controls.Add(lblGenerators);
            tabGenerators.Controls.Add(btnToggleGenerators);
            tabGenerators.Controls.Add(lblSQLTables);
            tabGenerators.Controls.Add(clbGenerators);
            tabGenerators.Controls.Add(clbSqlTables);
            resources.ApplyResources(tabGenerators, "tabGenerators");
            tabGenerators.Name = "tabGenerators";
            tabGenerators.UseVisualStyleBackColor = true;
            // 
            // btnToggleSql
            // 
            resources.ApplyResources(btnToggleSql, "btnToggleSql");
            btnToggleSql.Name = "btnToggleSql";
            btnToggleSql.UseVisualStyleBackColor = true;
            btnToggleSql.Click += btnToggleSql_Click;
            // 
            // lblGenerators
            // 
            resources.ApplyResources(lblGenerators, "lblGenerators");
            lblGenerators.Name = "lblGenerators";
            // 
            // btnToggleGenerators
            // 
            resources.ApplyResources(btnToggleGenerators, "btnToggleGenerators");
            btnToggleGenerators.Name = "btnToggleGenerators";
            btnToggleGenerators.UseVisualStyleBackColor = true;
            btnToggleGenerators.Click += btnToggleGenerators_Click;
            // 
            // lblSQLTables
            // 
            resources.ApplyResources(lblSQLTables, "lblSQLTables");
            lblSQLTables.Name = "lblSQLTables";
            // 
            // clbGenerators
            // 
            resources.ApplyResources(clbGenerators, "clbGenerators");
            clbGenerators.CheckOnClick = true;
            clbGenerators.FormattingEnabled = true;
            clbGenerators.Name = "clbGenerators";
            // 
            // clbSqlTables
            // 
            resources.ApplyResources(clbSqlTables, "clbSqlTables");
            clbSqlTables.CheckOnClick = true;
            clbSqlTables.FormattingEnabled = true;
            clbSqlTables.Name = "clbSqlTables";
            clbSqlTables.ItemCheck += cblSqlTables_ItemCheck;
            // 
            // tabServer
            // 
            tabServer.Controls.Add(btnUseDefaultConn);
            tabServer.Controls.Add(txtConn);
            tabServer.Controls.Add(lblLogon);
            tabServer.Controls.Add(cmbDatabaseList);
            tabServer.Controls.Add(lblDatabase);
            tabServer.Controls.Add(btnConnect);
            resources.ApplyResources(tabServer, "tabServer");
            tabServer.Name = "tabServer";
            tabServer.UseVisualStyleBackColor = true;
            // 
            // btnUseDefaultConn
            // 
            resources.ApplyResources(btnUseDefaultConn, "btnUseDefaultConn");
            btnUseDefaultConn.Name = "btnUseDefaultConn";
            btnUseDefaultConn.UseVisualStyleBackColor = true;
            btnUseDefaultConn.Click += btnUseDefaultConn_Click;
            // 
            // txtConn
            // 
            resources.ApplyResources(txtConn, "txtConn");
            txtConn.Name = "txtConn";
            txtConn.TextChanged += txtConn_TextChanged;
            // 
            // lblLogon
            // 
            resources.ApplyResources(lblLogon, "lblLogon");
            lblLogon.Name = "lblLogon";
            // 
            // cmbDatabaseList
            // 
            resources.ApplyResources(cmbDatabaseList, "cmbDatabaseList");
            cmbDatabaseList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbDatabaseList.FormattingEnabled = true;
            cmbDatabaseList.Name = "cmbDatabaseList";
            cmbDatabaseList.SelectedIndexChanged += cmbDatabaseList_SelectedIndexChanged;
            // 
            // lblDatabase
            // 
            resources.ApplyResources(lblDatabase, "lblDatabase");
            lblDatabase.Name = "lblDatabase";
            // 
            // btnConnect
            // 
            resources.ApplyResources(btnConnect, "btnConnect");
            btnConnect.Name = "btnConnect";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_Click;
            // 
            // tabcontrolAutoCodeGen
            // 
            resources.ApplyResources(tabcontrolAutoCodeGen, "tabcontrolAutoCodeGen");
            tabcontrolAutoCodeGen.Controls.Add(tabServer);
            tabcontrolAutoCodeGen.Controls.Add(tabGenerators);
            tabcontrolAutoCodeGen.Controls.Add(tabExport);
            tabcontrolAutoCodeGen.Controls.Add(tabOutput);
            tabcontrolAutoCodeGen.Controls.Add(tabAbout);
            tabcontrolAutoCodeGen.Name = "tabcontrolAutoCodeGen";
            tabcontrolAutoCodeGen.SelectedIndex = 0;
            // 
            // lvMessaging
            // 
            resources.ApplyResources(lvMessaging, "lvMessaging");
            lvMessaging.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { Message });
            lvMessaging.MultiSelect = false;
            lvMessaging.Name = "lvMessaging";
            lvMessaging.UseCompatibleStateImageBehavior = false;
            lvMessaging.View = System.Windows.Forms.View.Details;
            // 
            // Message
            // 
            resources.ApplyResources(Message, "Message");
            // 
            // lblMessages
            // 
            resources.ApplyResources(lblMessages, "lblMessages");
            lblMessages.Name = "lblMessages";
            // 
            // btnOpenOutputDirectory
            // 
            resources.ApplyResources(btnOpenOutputDirectory, "btnOpenOutputDirectory");
            btnOpenOutputDirectory.Name = "btnOpenOutputDirectory";
            btnOpenOutputDirectory.UseVisualStyleBackColor = true;
            btnOpenOutputDirectory.Click += btnOpenOutputDirectory_Click;
            // 
            // Main
            // 
            AcceptButton = btnGenerateCode;
            resources.ApplyResources(this, "$this");
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.SystemColors.Control;
            Controls.Add(btnOpenOutputDirectory);
            Controls.Add(lblMessages);
            Controls.Add(lvMessaging);
            Controls.Add(btnResetCurrentTab);
            Controls.Add(btnGenerateCode);
            Controls.Add(tabcontrolAutoCodeGen);
            Name = "Main";
            FormClosing += Main_FormClosing;
            tabAbout.ResumeLayout(false);
            tabAbout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pbxLogo).EndInit();
            tabOutput.ResumeLayout(false);
            tabOutput.PerformLayout();
            tabExport.ResumeLayout(false);
            tabExport.PerformLayout();
            tabGenerators.ResumeLayout(false);
            tabGenerators.PerformLayout();
            tabServer.ResumeLayout(false);
            tabServer.PerformLayout();
            tabcontrolAutoCodeGen.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox txtOutputPath;
        private System.Windows.Forms.TextBox txtConn;
        private System.Windows.Forms.Label lblApplicationTitle;
        private System.Windows.Forms.Label lblOutputPath;
        private System.Windows.Forms.Label lblOutputOptions;
        private System.Windows.Forms.Label lblSQLTables;
        private System.Windows.Forms.Label lblLogon;
        private System.Windows.Forms.Label lblDatabase;
        private System.Windows.Forms.Label lblExportTables;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.Label lblAuthor;

        private System.Windows.Forms.ComboBox cmbDatabaseList;
        private System.Windows.Forms.PictureBox pbxLogo;
        private System.Windows.Forms.LinkLabel lnkWebLink;

        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnSetDirectory;
        private System.Windows.Forms.Button btnGenerateCode;
        private System.Windows.Forms.Button btnResetCurrentTab;
        private System.Windows.Forms.Button btnToggleGenerators;
        private System.Windows.Forms.Button btnToggleExportSqlTables;
        private System.Windows.Forms.Button btnToggleOutput;

        private System.Windows.Forms.TabControl tabcontrolAutoCodeGen;
        private System.Windows.Forms.TabPage tabAbout;
        private System.Windows.Forms.TabPage tabExport;
        private System.Windows.Forms.TabPage tabGenerators;
        private System.Windows.Forms.TabPage tabOutput;
        private System.Windows.Forms.TabPage tabServer;

        private System.Windows.Forms.CheckedListBox clbExportSqlTables;
        private System.Windows.Forms.CheckedListBox clbOutputOptions;
        private System.Windows.Forms.CheckedListBox clbGenerators;
        private System.Windows.Forms.CheckedListBox clbSqlTables;
        private System.Windows.Forms.LinkLabel lnkEmail;
        private System.Windows.Forms.ListView lvMessaging;
        private System.Windows.Forms.ColumnHeader Message;
        private System.Windows.Forms.Label lblMessages;
        private System.Windows.Forms.Label lblXml;
        private System.Windows.Forms.CheckedListBox clbExportOptions;
        private System.Windows.Forms.Button btnToggleExportObjects;
        private System.Windows.Forms.Label lblCopyright;
        private System.Windows.Forms.Button btnOpenOutputDirectory;
        private System.Windows.Forms.Label lblGenerators;
        private System.Windows.Forms.Button btnToggleSql;
        private System.Windows.Forms.Button btnUseDefaultConn;
    }
}