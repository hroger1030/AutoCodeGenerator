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
            lblConnectStatus = new System.Windows.Forms.Label();
            tabAbout = new System.Windows.Forms.TabPage();
            lblCopyright = new System.Windows.Forms.Label();
            lnkEmail = new System.Windows.Forms.LinkLabel();
            lnkWebLink = new System.Windows.Forms.LinkLabel();
            pbxLogo = new System.Windows.Forms.PictureBox();
            lblAuthor = new System.Windows.Forms.Label();
            lblVersion = new System.Windows.Forms.Label();
            lblAbout = new System.Windows.Forms.Label();
            lblApplicationTitle = new System.Windows.Forms.Label();
            tabOutput = new System.Windows.Forms.TabPage();
            btnCheckRegex = new System.Windows.Forms.Button();
            lblRegexResult = new System.Windows.Forms.Label();
            txtTableNameRegex = new System.Windows.Forms.TextBox();
            lblTableNameRegex = new System.Windows.Forms.Label();
            btnSetDirectory = new System.Windows.Forms.Button();
            btnToggleOutput = new System.Windows.Forms.Button();
            lblOutputPath = new System.Windows.Forms.Label();
            txtOutputPath = new System.Windows.Forms.TextBox();
            lblOutputOptions = new System.Windows.Forms.Label();
            clbOutputOptions = new System.Windows.Forms.CheckedListBox();
            tabXml = new System.Windows.Forms.TabPage();
            btnToggleWebXmlObjects = new System.Windows.Forms.Button();
            lblXml = new System.Windows.Forms.Label();
            clbXmlOptionsTables = new System.Windows.Forms.CheckedListBox();
            btnToggleWebXmlSqlTables = new System.Windows.Forms.Button();
            lblXMLExportTables = new System.Windows.Forms.Label();
            clbXmlSqlTables = new System.Windows.Forms.CheckedListBox();
            tabCsharp = new System.Windows.Forms.TabPage();
            panCSharpOptions = new System.Windows.Forms.Panel();
            btnAddNamespace = new System.Windows.Forms.Button();
            cboNamespaceIncludes = new System.Windows.Forms.ComboBox();
            label1 = new System.Windows.Forms.Label();
            btnToggleCsharpSqlTables = new System.Windows.Forms.Button();
            btnToggleCSharpObjects = new System.Windows.Forms.Button();
            lblCSharpObjects = new System.Windows.Forms.Label();
            lblCSharpTables = new System.Windows.Forms.Label();
            clbCsharpSqlTables = new System.Windows.Forms.CheckedListBox();
            clbCsharpObjects = new System.Windows.Forms.CheckedListBox();
            btnToggleAspSqlTables = new System.Windows.Forms.Button();
            tabSql = new System.Windows.Forms.TabPage();
            lblSQLVersion = new System.Windows.Forms.Label();
            cmbSQLVersion = new System.Windows.Forms.ComboBox();
            btnToggleTsqlSqlObjects = new System.Windows.Forms.Button();
            btnToggleTsqlSqlTables = new System.Windows.Forms.Button();
            lblSQLObjects = new System.Windows.Forms.Label();
            lblSQLTables = new System.Windows.Forms.Label();
            clbTsqlSqlObjects = new System.Windows.Forms.CheckedListBox();
            clbTsqlSqlTables = new System.Windows.Forms.CheckedListBox();
            tabServer = new System.Windows.Forms.TabPage();
            ckbLocalDb = new System.Windows.Forms.CheckBox();
            lblPassword = new System.Windows.Forms.Label();
            txtPassword = new System.Windows.Forms.TextBox();
            txtLogon = new System.Windows.Forms.TextBox();
            txtServerName = new System.Windows.Forms.TextBox();
            lblLogon = new System.Windows.Forms.Label();
            cmbDatabaseList = new System.Windows.Forms.ComboBox();
            lblDatabase = new System.Windows.Forms.Label();
            btnConnect = new System.Windows.Forms.Button();
            lblServerName = new System.Windows.Forms.Label();
            tabcontrolAutoCodeGen = new System.Windows.Forms.TabControl();
            tabWinform = new System.Windows.Forms.TabPage();
            btnToggleWinformObjects = new System.Windows.Forms.Button();
            label2 = new System.Windows.Forms.Label();
            clbWinformObjects = new System.Windows.Forms.CheckedListBox();
            label3 = new System.Windows.Forms.Label();
            btnToggleWinformSqlTables = new System.Windows.Forms.Button();
            clbWinformSqlTables = new System.Windows.Forms.CheckedListBox();
            tabWebService = new System.Windows.Forms.TabPage();
            btnToggleWebServicesSqlTables = new System.Windows.Forms.Button();
            btnToggleWebServiceObjects = new System.Windows.Forms.Button();
            lblWebServiceObjects = new System.Windows.Forms.Label();
            clbWebServiceObjects = new System.Windows.Forms.CheckedListBox();
            lblWebServiceTables = new System.Windows.Forms.Label();
            clbWebServiceSqlTables = new System.Windows.Forms.CheckedListBox();
            tabAsp = new System.Windows.Forms.TabPage();
            lblASPTables = new System.Windows.Forms.Label();
            clbAspSqlTables = new System.Windows.Forms.CheckedListBox();
            lblASPObjects = new System.Windows.Forms.Label();
            btnToggleASPObjects = new System.Windows.Forms.Button();
            clbAspObjects = new System.Windows.Forms.CheckedListBox();
            lvMessaging = new System.Windows.Forms.ListView();
            Message = new System.Windows.Forms.ColumnHeader();
            lblMessages = new System.Windows.Forms.Label();
            btnOpenOutputDirectory = new System.Windows.Forms.Button();
            tabAbout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pbxLogo).BeginInit();
            tabOutput.SuspendLayout();
            tabXml.SuspendLayout();
            tabCsharp.SuspendLayout();
            panCSharpOptions.SuspendLayout();
            tabSql.SuspendLayout();
            tabServer.SuspendLayout();
            tabcontrolAutoCodeGen.SuspendLayout();
            tabWinform.SuspendLayout();
            tabWebService.SuspendLayout();
            tabAsp.SuspendLayout();
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
            // lblConnectStatus
            // 
            resources.ApplyResources(lblConnectStatus, "lblConnectStatus");
            lblConnectStatus.Name = "lblConnectStatus";
            // 
            // tabAbout
            // 
            tabAbout.Controls.Add(lblCopyright);
            tabAbout.Controls.Add(lnkEmail);
            tabAbout.Controls.Add(lnkWebLink);
            tabAbout.Controls.Add(pbxLogo);
            tabAbout.Controls.Add(lblAuthor);
            tabAbout.Controls.Add(lblVersion);
            tabAbout.Controls.Add(lblAbout);
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
            // lblAbout
            // 
            resources.ApplyResources(lblAbout, "lblAbout");
            lblAbout.Name = "lblAbout";
            // 
            // lblApplicationTitle
            // 
            resources.ApplyResources(lblApplicationTitle, "lblApplicationTitle");
            lblApplicationTitle.Name = "lblApplicationTitle";
            // 
            // tabOutput
            // 
            tabOutput.Controls.Add(btnCheckRegex);
            tabOutput.Controls.Add(lblRegexResult);
            tabOutput.Controls.Add(txtTableNameRegex);
            tabOutput.Controls.Add(lblTableNameRegex);
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
            // btnCheckRegex
            // 
            resources.ApplyResources(btnCheckRegex, "btnCheckRegex");
            btnCheckRegex.Name = "btnCheckRegex";
            btnCheckRegex.UseVisualStyleBackColor = true;
            btnCheckRegex.Click += btnCheckRegex_Click;
            // 
            // lblRegexResult
            // 
            resources.ApplyResources(lblRegexResult, "lblRegexResult");
            lblRegexResult.Name = "lblRegexResult";
            // 
            // txtTableNameRegex
            // 
            resources.ApplyResources(txtTableNameRegex, "txtTableNameRegex");
            txtTableNameRegex.Name = "txtTableNameRegex";
            // 
            // lblTableNameRegex
            // 
            resources.ApplyResources(lblTableNameRegex, "lblTableNameRegex");
            lblTableNameRegex.Name = "lblTableNameRegex";
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
            // tabXml
            // 
            tabXml.Controls.Add(btnToggleWebXmlObjects);
            tabXml.Controls.Add(lblXml);
            tabXml.Controls.Add(clbXmlOptionsTables);
            tabXml.Controls.Add(btnToggleWebXmlSqlTables);
            tabXml.Controls.Add(lblXMLExportTables);
            tabXml.Controls.Add(clbXmlSqlTables);
            resources.ApplyResources(tabXml, "tabXml");
            tabXml.Name = "tabXml";
            tabXml.UseVisualStyleBackColor = true;
            // 
            // btnToggleWebXmlObjects
            // 
            resources.ApplyResources(btnToggleWebXmlObjects, "btnToggleWebXmlObjects");
            btnToggleWebXmlObjects.Name = "btnToggleWebXmlObjects";
            btnToggleWebXmlObjects.UseVisualStyleBackColor = true;
            btnToggleWebXmlObjects.Click += btnToggleWebXmlObjects_Click;
            // 
            // lblXml
            // 
            resources.ApplyResources(lblXml, "lblXml");
            lblXml.Name = "lblXml";
            // 
            // clbXmlOptionsTables
            // 
            resources.ApplyResources(clbXmlOptionsTables, "clbXmlOptionsTables");
            clbXmlOptionsTables.CheckOnClick = true;
            clbXmlOptionsTables.FormattingEnabled = true;
            clbXmlOptionsTables.Name = "clbXmlOptionsTables";
            clbXmlOptionsTables.Sorted = true;
            clbXmlOptionsTables.ItemCheck += clbXmlOptionTables_ItemCheck;
            // 
            // btnToggleWebXmlSqlTables
            // 
            resources.ApplyResources(btnToggleWebXmlSqlTables, "btnToggleWebXmlSqlTables");
            btnToggleWebXmlSqlTables.Name = "btnToggleWebXmlSqlTables";
            btnToggleWebXmlSqlTables.UseVisualStyleBackColor = true;
            btnToggleWebXmlSqlTables.Click += btnToggleWebXmlSqlTables_Click;
            // 
            // lblXMLExportTables
            // 
            resources.ApplyResources(lblXMLExportTables, "lblXMLExportTables");
            lblXMLExportTables.Name = "lblXMLExportTables";
            // 
            // clbXmlSqlTables
            // 
            resources.ApplyResources(clbXmlSqlTables, "clbXmlSqlTables");
            clbXmlSqlTables.CheckOnClick = true;
            clbXmlSqlTables.FormattingEnabled = true;
            clbXmlSqlTables.Name = "clbXmlSqlTables";
            clbXmlSqlTables.ItemCheck += clbXmlSqlTables_ItemCheck;
            // 
            // tabCsharp
            // 
            tabCsharp.Controls.Add(panCSharpOptions);
            tabCsharp.Controls.Add(btnToggleCsharpSqlTables);
            tabCsharp.Controls.Add(btnToggleCSharpObjects);
            tabCsharp.Controls.Add(lblCSharpObjects);
            tabCsharp.Controls.Add(lblCSharpTables);
            tabCsharp.Controls.Add(clbCsharpSqlTables);
            tabCsharp.Controls.Add(clbCsharpObjects);
            resources.ApplyResources(tabCsharp, "tabCsharp");
            tabCsharp.Name = "tabCsharp";
            tabCsharp.UseVisualStyleBackColor = true;
            // 
            // panCSharpOptions
            // 
            resources.ApplyResources(panCSharpOptions, "panCSharpOptions");
            panCSharpOptions.Controls.Add(btnAddNamespace);
            panCSharpOptions.Controls.Add(cboNamespaceIncludes);
            panCSharpOptions.Controls.Add(label1);
            panCSharpOptions.Name = "panCSharpOptions";
            // 
            // btnAddNamespace
            // 
            resources.ApplyResources(btnAddNamespace, "btnAddNamespace");
            btnAddNamespace.Name = "btnAddNamespace";
            btnAddNamespace.UseVisualStyleBackColor = true;
            btnAddNamespace.Click += btnAddNamespace_Click;
            // 
            // cboNamespaceIncludes
            // 
            resources.ApplyResources(cboNamespaceIncludes, "cboNamespaceIncludes");
            cboNamespaceIncludes.FormattingEnabled = true;
            cboNamespaceIncludes.Name = "cboNamespaceIncludes";
            cboNamespaceIncludes.KeyDown += cboNamespaceIncludes_KeyDown;
            // 
            // label1
            // 
            resources.ApplyResources(label1, "label1");
            label1.Name = "label1";
            // 
            // btnToggleCsharpSqlTables
            // 
            resources.ApplyResources(btnToggleCsharpSqlTables, "btnToggleCsharpSqlTables");
            btnToggleCsharpSqlTables.Name = "btnToggleCsharpSqlTables";
            btnToggleCsharpSqlTables.UseVisualStyleBackColor = true;
            btnToggleCsharpSqlTables.Click += btnToggleCsharpSqlTables_Click;
            // 
            // btnToggleCSharpObjects
            // 
            resources.ApplyResources(btnToggleCSharpObjects, "btnToggleCSharpObjects");
            btnToggleCSharpObjects.Name = "btnToggleCSharpObjects";
            btnToggleCSharpObjects.UseVisualStyleBackColor = true;
            btnToggleCSharpObjects.Click += btnToggleCSharpObjects_Click;
            // 
            // lblCSharpObjects
            // 
            resources.ApplyResources(lblCSharpObjects, "lblCSharpObjects");
            lblCSharpObjects.Name = "lblCSharpObjects";
            // 
            // lblCSharpTables
            // 
            resources.ApplyResources(lblCSharpTables, "lblCSharpTables");
            lblCSharpTables.Name = "lblCSharpTables";
            // 
            // clbCsharpSqlTables
            // 
            resources.ApplyResources(clbCsharpSqlTables, "clbCsharpSqlTables");
            clbCsharpSqlTables.CheckOnClick = true;
            clbCsharpSqlTables.FormattingEnabled = true;
            clbCsharpSqlTables.Name = "clbCsharpSqlTables";
            clbCsharpSqlTables.ItemCheck += clbCsharpSqlTables_ItemCheck;
            // 
            // clbCsharpObjects
            // 
            resources.ApplyResources(clbCsharpObjects, "clbCsharpObjects");
            clbCsharpObjects.CheckOnClick = true;
            clbCsharpObjects.FormattingEnabled = true;
            clbCsharpObjects.Name = "clbCsharpObjects";
            clbCsharpObjects.ItemCheck += clbCSharpObjects_ItemCheck;
            // 
            // btnToggleAspSqlTables
            // 
            resources.ApplyResources(btnToggleAspSqlTables, "btnToggleAspSqlTables");
            btnToggleAspSqlTables.Name = "btnToggleAspSqlTables";
            btnToggleAspSqlTables.UseVisualStyleBackColor = true;
            btnToggleAspSqlTables.Click += btnToggleAspSqlTables_Click;
            // 
            // tabSql
            // 
            tabSql.Controls.Add(lblSQLVersion);
            tabSql.Controls.Add(cmbSQLVersion);
            tabSql.Controls.Add(btnToggleTsqlSqlObjects);
            tabSql.Controls.Add(btnToggleTsqlSqlTables);
            tabSql.Controls.Add(lblSQLObjects);
            tabSql.Controls.Add(lblSQLTables);
            tabSql.Controls.Add(clbTsqlSqlObjects);
            tabSql.Controls.Add(clbTsqlSqlTables);
            resources.ApplyResources(tabSql, "tabSql");
            tabSql.Name = "tabSql";
            tabSql.UseVisualStyleBackColor = true;
            // 
            // lblSQLVersion
            // 
            resources.ApplyResources(lblSQLVersion, "lblSQLVersion");
            lblSQLVersion.Name = "lblSQLVersion";
            // 
            // cmbSQLVersion
            // 
            cmbSQLVersion.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            resources.ApplyResources(cmbSQLVersion, "cmbSQLVersion");
            cmbSQLVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbSQLVersion.FormattingEnabled = true;
            cmbSQLVersion.Name = "cmbSQLVersion";
            // 
            // btnToggleTsqlSqlObjects
            // 
            resources.ApplyResources(btnToggleTsqlSqlObjects, "btnToggleTsqlSqlObjects");
            btnToggleTsqlSqlObjects.Name = "btnToggleTsqlSqlObjects";
            btnToggleTsqlSqlObjects.UseVisualStyleBackColor = true;
            btnToggleTsqlSqlObjects.Click += btnToggleTsqlSqlObjects_Click;
            // 
            // btnToggleTsqlSqlTables
            // 
            resources.ApplyResources(btnToggleTsqlSqlTables, "btnToggleTsqlSqlTables");
            btnToggleTsqlSqlTables.Name = "btnToggleTsqlSqlTables";
            btnToggleTsqlSqlTables.UseVisualStyleBackColor = true;
            btnToggleTsqlSqlTables.Click += btnToggleTsqlSqlTables_Click;
            // 
            // lblSQLObjects
            // 
            resources.ApplyResources(lblSQLObjects, "lblSQLObjects");
            lblSQLObjects.Name = "lblSQLObjects";
            // 
            // lblSQLTables
            // 
            resources.ApplyResources(lblSQLTables, "lblSQLTables");
            lblSQLTables.Name = "lblSQLTables";
            // 
            // clbTsqlSqlObjects
            // 
            resources.ApplyResources(clbTsqlSqlObjects, "clbTsqlSqlObjects");
            clbTsqlSqlObjects.CheckOnClick = true;
            clbTsqlSqlObjects.FormattingEnabled = true;
            clbTsqlSqlObjects.Name = "clbTsqlSqlObjects";
            clbTsqlSqlObjects.ItemCheck += clbTsqlSqlObjects_ItemCheck;
            // 
            // clbTsqlSqlTables
            // 
            resources.ApplyResources(clbTsqlSqlTables, "clbTsqlSqlTables");
            clbTsqlSqlTables.CheckOnClick = true;
            clbTsqlSqlTables.FormattingEnabled = true;
            clbTsqlSqlTables.Name = "clbTsqlSqlTables";
            clbTsqlSqlTables.ItemCheck += clbTsqlSqlTables_ItemCheck;
            // 
            // tabServer
            // 
            tabServer.Controls.Add(ckbLocalDb);
            tabServer.Controls.Add(lblPassword);
            tabServer.Controls.Add(txtPassword);
            tabServer.Controls.Add(txtLogon);
            tabServer.Controls.Add(txtServerName);
            tabServer.Controls.Add(lblLogon);
            tabServer.Controls.Add(cmbDatabaseList);
            tabServer.Controls.Add(lblDatabase);
            tabServer.Controls.Add(btnConnect);
            tabServer.Controls.Add(lblServerName);
            resources.ApplyResources(tabServer, "tabServer");
            tabServer.Name = "tabServer";
            tabServer.UseVisualStyleBackColor = true;
            // 
            // ckbLocalDb
            // 
            resources.ApplyResources(ckbLocalDb, "ckbLocalDb");
            ckbLocalDb.Checked = true;
            ckbLocalDb.CheckState = System.Windows.Forms.CheckState.Checked;
            ckbLocalDb.Name = "ckbLocalDb";
            ckbLocalDb.UseVisualStyleBackColor = true;
            ckbLocalDb.CheckedChanged += ckbLocalDb_CheckedChanged;
            // 
            // lblPassword
            // 
            resources.ApplyResources(lblPassword, "lblPassword");
            lblPassword.Name = "lblPassword";
            // 
            // txtPassword
            // 
            resources.ApplyResources(txtPassword, "txtPassword");
            txtPassword.Name = "txtPassword";
            txtPassword.TextChanged += txtPassword_TextChanged;
            // 
            // txtLogon
            // 
            resources.ApplyResources(txtLogon, "txtLogon");
            txtLogon.Name = "txtLogon";
            txtLogon.TextChanged += txtLogon_TextChanged;
            // 
            // txtServerName
            // 
            resources.ApplyResources(txtServerName, "txtServerName");
            txtServerName.Name = "txtServerName";
            txtServerName.TextChanged += txtServerName_TextChanged;
            // 
            // lblLogon
            // 
            resources.ApplyResources(lblLogon, "lblLogon");
            lblLogon.Name = "lblLogon";
            // 
            // cmbDatabaseList
            // 
            cmbDatabaseList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbDatabaseList.FormattingEnabled = true;
            resources.ApplyResources(cmbDatabaseList, "cmbDatabaseList");
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
            // lblServerName
            // 
            resources.ApplyResources(lblServerName, "lblServerName");
            lblServerName.Name = "lblServerName";
            // 
            // tabcontrolAutoCodeGen
            // 
            resources.ApplyResources(tabcontrolAutoCodeGen, "tabcontrolAutoCodeGen");
            tabcontrolAutoCodeGen.Controls.Add(tabServer);
            tabcontrolAutoCodeGen.Controls.Add(tabSql);
            tabcontrolAutoCodeGen.Controls.Add(tabCsharp);
            tabcontrolAutoCodeGen.Controls.Add(tabWinform);
            tabcontrolAutoCodeGen.Controls.Add(tabWebService);
            tabcontrolAutoCodeGen.Controls.Add(tabAsp);
            tabcontrolAutoCodeGen.Controls.Add(tabXml);
            tabcontrolAutoCodeGen.Controls.Add(tabOutput);
            tabcontrolAutoCodeGen.Controls.Add(tabAbout);
            tabcontrolAutoCodeGen.Name = "tabcontrolAutoCodeGen";
            tabcontrolAutoCodeGen.SelectedIndex = 0;
            // 
            // tabWinform
            // 
            tabWinform.Controls.Add(btnToggleWinformObjects);
            tabWinform.Controls.Add(label2);
            tabWinform.Controls.Add(clbWinformObjects);
            tabWinform.Controls.Add(label3);
            tabWinform.Controls.Add(btnToggleWinformSqlTables);
            tabWinform.Controls.Add(clbWinformSqlTables);
            resources.ApplyResources(tabWinform, "tabWinform");
            tabWinform.Name = "tabWinform";
            tabWinform.UseVisualStyleBackColor = true;
            // 
            // btnToggleWinformObjects
            // 
            resources.ApplyResources(btnToggleWinformObjects, "btnToggleWinformObjects");
            btnToggleWinformObjects.Name = "btnToggleWinformObjects";
            btnToggleWinformObjects.UseVisualStyleBackColor = true;
            btnToggleWinformObjects.Click += btnToggleWinformObjects_Click;
            // 
            // label2
            // 
            resources.ApplyResources(label2, "label2");
            label2.Name = "label2";
            // 
            // clbWinformObjects
            // 
            resources.ApplyResources(clbWinformObjects, "clbWinformObjects");
            clbWinformObjects.CheckOnClick = true;
            clbWinformObjects.FormattingEnabled = true;
            clbWinformObjects.Name = "clbWinformObjects";
            clbWinformObjects.ItemCheck += clbWinformObjects_ItemCheck;
            // 
            // label3
            // 
            resources.ApplyResources(label3, "label3");
            label3.Name = "label3";
            // 
            // btnToggleWinformSqlTables
            // 
            resources.ApplyResources(btnToggleWinformSqlTables, "btnToggleWinformSqlTables");
            btnToggleWinformSqlTables.Name = "btnToggleWinformSqlTables";
            btnToggleWinformSqlTables.UseVisualStyleBackColor = true;
            btnToggleWinformSqlTables.Click += btnToggleWinformSqlTables_Click;
            // 
            // clbWinformSqlTables
            // 
            resources.ApplyResources(clbWinformSqlTables, "clbWinformSqlTables");
            clbWinformSqlTables.CheckOnClick = true;
            clbWinformSqlTables.FormattingEnabled = true;
            clbWinformSqlTables.Name = "clbWinformSqlTables";
            clbWinformSqlTables.ItemCheck += clbWinformSqlTables_ItemCheck;
            // 
            // tabWebService
            // 
            tabWebService.Controls.Add(btnToggleWebServicesSqlTables);
            tabWebService.Controls.Add(btnToggleWebServiceObjects);
            tabWebService.Controls.Add(lblWebServiceObjects);
            tabWebService.Controls.Add(clbWebServiceObjects);
            tabWebService.Controls.Add(lblWebServiceTables);
            tabWebService.Controls.Add(clbWebServiceSqlTables);
            resources.ApplyResources(tabWebService, "tabWebService");
            tabWebService.Name = "tabWebService";
            tabWebService.UseVisualStyleBackColor = true;
            // 
            // btnToggleWebServicesSqlTables
            // 
            resources.ApplyResources(btnToggleWebServicesSqlTables, "btnToggleWebServicesSqlTables");
            btnToggleWebServicesSqlTables.Name = "btnToggleWebServicesSqlTables";
            btnToggleWebServicesSqlTables.UseVisualStyleBackColor = true;
            btnToggleWebServicesSqlTables.Click += btnToggleWebServicesSqlTables_Click;
            // 
            // btnToggleWebServiceObjects
            // 
            resources.ApplyResources(btnToggleWebServiceObjects, "btnToggleWebServiceObjects");
            btnToggleWebServiceObjects.Name = "btnToggleWebServiceObjects";
            btnToggleWebServiceObjects.UseVisualStyleBackColor = true;
            btnToggleWebServiceObjects.Click += btnToggleWebServiceObjects_Click;
            // 
            // lblWebServiceObjects
            // 
            resources.ApplyResources(lblWebServiceObjects, "lblWebServiceObjects");
            lblWebServiceObjects.Name = "lblWebServiceObjects";
            // 
            // clbWebServiceObjects
            // 
            resources.ApplyResources(clbWebServiceObjects, "clbWebServiceObjects");
            clbWebServiceObjects.CheckOnClick = true;
            clbWebServiceObjects.FormattingEnabled = true;
            clbWebServiceObjects.Name = "clbWebServiceObjects";
            clbWebServiceObjects.ItemCheck += clbWebServiceObjects_ItemCheck;
            // 
            // lblWebServiceTables
            // 
            resources.ApplyResources(lblWebServiceTables, "lblWebServiceTables");
            lblWebServiceTables.Name = "lblWebServiceTables";
            // 
            // clbWebServiceSqlTables
            // 
            resources.ApplyResources(clbWebServiceSqlTables, "clbWebServiceSqlTables");
            clbWebServiceSqlTables.CheckOnClick = true;
            clbWebServiceSqlTables.FormattingEnabled = true;
            clbWebServiceSqlTables.Name = "clbWebServiceSqlTables";
            clbWebServiceSqlTables.ItemCheck += clbWebServiceSqlTables_ItemCheck;
            // 
            // tabAsp
            // 
            tabAsp.Controls.Add(btnToggleAspSqlTables);
            tabAsp.Controls.Add(lblASPTables);
            tabAsp.Controls.Add(clbAspSqlTables);
            tabAsp.Controls.Add(lblASPObjects);
            tabAsp.Controls.Add(btnToggleASPObjects);
            tabAsp.Controls.Add(clbAspObjects);
            resources.ApplyResources(tabAsp, "tabAsp");
            tabAsp.Name = "tabAsp";
            tabAsp.UseVisualStyleBackColor = true;
            // 
            // lblASPTables
            // 
            resources.ApplyResources(lblASPTables, "lblASPTables");
            lblASPTables.Name = "lblASPTables";
            // 
            // clbAspSqlTables
            // 
            resources.ApplyResources(clbAspSqlTables, "clbAspSqlTables");
            clbAspSqlTables.CheckOnClick = true;
            clbAspSqlTables.FormattingEnabled = true;
            clbAspSqlTables.Name = "clbAspSqlTables";
            clbAspSqlTables.ItemCheck += clbAspSqlTables_ItemCheck;
            // 
            // lblASPObjects
            // 
            resources.ApplyResources(lblASPObjects, "lblASPObjects");
            lblASPObjects.Name = "lblASPObjects";
            // 
            // btnToggleASPObjects
            // 
            resources.ApplyResources(btnToggleASPObjects, "btnToggleASPObjects");
            btnToggleASPObjects.Name = "btnToggleASPObjects";
            btnToggleASPObjects.UseVisualStyleBackColor = true;
            btnToggleASPObjects.Click += btnToggleASPObjects_Click;
            // 
            // clbAspObjects
            // 
            resources.ApplyResources(clbAspObjects, "clbAspObjects");
            clbAspObjects.CheckOnClick = true;
            clbAspObjects.FormattingEnabled = true;
            clbAspObjects.Name = "clbAspObjects";
            clbAspObjects.Sorted = true;
            clbAspObjects.ItemCheck += clbAspObjects_ItemCheck;
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
            Controls.Add(lblConnectStatus);
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
            tabXml.ResumeLayout(false);
            tabXml.PerformLayout();
            tabCsharp.ResumeLayout(false);
            tabCsharp.PerformLayout();
            panCSharpOptions.ResumeLayout(false);
            panCSharpOptions.PerformLayout();
            tabSql.ResumeLayout(false);
            tabSql.PerformLayout();
            tabServer.ResumeLayout(false);
            tabServer.PerformLayout();
            tabcontrolAutoCodeGen.ResumeLayout(false);
            tabWinform.ResumeLayout(false);
            tabWinform.PerformLayout();
            tabWebService.ResumeLayout(false);
            tabWebService.PerformLayout();
            tabAsp.ResumeLayout(false);
            tabAsp.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox txtOutputPath;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.TextBox txtLogon;
        private System.Windows.Forms.TextBox txtServerName;
        private System.Windows.Forms.TextBox txtTableNameRegex;

        private System.Windows.Forms.Label lblConnectStatus;
        private System.Windows.Forms.Label lblAbout;
        private System.Windows.Forms.Label lblApplicationTitle;
        private System.Windows.Forms.Label lblOutputPath;
        private System.Windows.Forms.Label lblOutputOptions;
        private System.Windows.Forms.Label lblSQLObjects;
        private System.Windows.Forms.Label lblSQLTables;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.Label lblLogon;
        private System.Windows.Forms.Label lblDatabase;
        private System.Windows.Forms.Label lblServerName;
        private System.Windows.Forms.Label lblXMLExportTables;
        private System.Windows.Forms.Label lblCSharpTables;
        private System.Windows.Forms.Label lblCSharpObjects;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.Label lblAuthor;
        private System.Windows.Forms.Label lblWebServiceTables;
        private System.Windows.Forms.Label lblWebServiceObjects;
        private System.Windows.Forms.Label lblASPObjects;
        private System.Windows.Forms.Label lblASPTables;
        private System.Windows.Forms.Label lblRegexResult;
        private System.Windows.Forms.Label lblTableNameRegex;

        private System.Windows.Forms.ComboBox cmbDatabaseList;
        private System.Windows.Forms.CheckBox ckbLocalDb;
        private System.Windows.Forms.PictureBox pbxLogo;
        private System.Windows.Forms.LinkLabel lnkWebLink;

        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnSetDirectory;
        private System.Windows.Forms.Button btnGenerateCode;
        private System.Windows.Forms.Button btnCheckRegex;
        private System.Windows.Forms.Button btnResetCurrentTab;
        private System.Windows.Forms.Button btnToggleTsqlSqlObjects;
        private System.Windows.Forms.Button btnToggleTsqlSqlTables;
        private System.Windows.Forms.Button btnToggleWebXmlSqlTables;
        private System.Windows.Forms.Button btnToggleCSharpObjects;
        private System.Windows.Forms.Button btnToggleCsharpSqlTables;
        private System.Windows.Forms.Button btnToggleASPObjects;
        private System.Windows.Forms.Button btnToggleAspSqlTables;
        private System.Windows.Forms.Button btnToggleOutput;

        private System.Windows.Forms.TabControl tabcontrolAutoCodeGen;
        private System.Windows.Forms.TabPage tabAbout;
        private System.Windows.Forms.TabPage tabWebService;
        private System.Windows.Forms.TabPage tabAsp;
        private System.Windows.Forms.TabPage tabXml;
        private System.Windows.Forms.TabPage tabCsharp;
        private System.Windows.Forms.TabPage tabSql;
        private System.Windows.Forms.TabPage tabOutput;
        private System.Windows.Forms.TabPage tabServer;

        private System.Windows.Forms.CheckedListBox clbWebServiceObjects;
        private System.Windows.Forms.CheckedListBox clbAspSqlTables;
        private System.Windows.Forms.CheckedListBox clbWebServiceSqlTables;
        private System.Windows.Forms.CheckedListBox clbAspObjects;
        private System.Windows.Forms.CheckedListBox clbXmlSqlTables;
        private System.Windows.Forms.CheckedListBox clbCsharpSqlTables;
        private System.Windows.Forms.CheckedListBox clbCsharpObjects;
        private System.Windows.Forms.CheckedListBox clbOutputOptions;
        private System.Windows.Forms.CheckedListBox clbTsqlSqlObjects;
        private System.Windows.Forms.CheckedListBox clbTsqlSqlTables;
        private System.Windows.Forms.LinkLabel lnkEmail;
        private System.Windows.Forms.ListView lvMessaging;
        private System.Windows.Forms.ColumnHeader Message;
        private System.Windows.Forms.Label lblMessages;
        private System.Windows.Forms.Label lblSQLVersion;
        private System.Windows.Forms.ComboBox cmbSQLVersion;
        private System.Windows.Forms.Panel panCSharpOptions;
        private System.Windows.Forms.ComboBox cboNamespaceIncludes;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnAddNamespace;
        private System.Windows.Forms.TabPage tabWinform;
        private System.Windows.Forms.Button btnToggleWinformObjects;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckedListBox clbWinformObjects;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnToggleWinformSqlTables;
        private System.Windows.Forms.CheckedListBox clbWinformSqlTables;
        private System.Windows.Forms.Button btnToggleWebServiceObjects;
        private System.Windows.Forms.Button btnToggleWebServicesSqlTables;
        private System.Windows.Forms.Label lblXml;
        private System.Windows.Forms.CheckedListBox clbXmlOptionsTables;
        private System.Windows.Forms.Button btnToggleWebXmlObjects;
        private System.Windows.Forms.Label lblCopyright;
        private System.Windows.Forms.Button btnOpenOutputDirectory;
    }
}