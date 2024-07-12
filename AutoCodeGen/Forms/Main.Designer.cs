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
            tabExport = new System.Windows.Forms.TabPage();
            btnToggleExportObjects = new System.Windows.Forms.Button();
            lblXml = new System.Windows.Forms.Label();
            clbExportOptionsTables = new System.Windows.Forms.CheckedListBox();
            btnToggleExportSqlTables = new System.Windows.Forms.Button();
            lblXMLExportTables = new System.Windows.Forms.Label();
            clbExportSqlTables = new System.Windows.Forms.CheckedListBox();
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
            btnToggleReactSqlTables = new System.Windows.Forms.Button();
            tabSql = new System.Windows.Forms.TabPage();
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
            tabWebService = new System.Windows.Forms.TabPage();
            btnToggleWebServicesSqlTables = new System.Windows.Forms.Button();
            btnToggleWebServiceObjects = new System.Windows.Forms.Button();
            lblWebServiceObjects = new System.Windows.Forms.Label();
            clbWebServiceObjects = new System.Windows.Forms.CheckedListBox();
            lblWebServiceTables = new System.Windows.Forms.Label();
            clbWebServiceSqlTables = new System.Windows.Forms.CheckedListBox();
            tabReact = new System.Windows.Forms.TabPage();
            lblASPTables = new System.Windows.Forms.Label();
            clbReactSqlTables = new System.Windows.Forms.CheckedListBox();
            lblASPObjects = new System.Windows.Forms.Label();
            btnToggleASPObjects = new System.Windows.Forms.Button();
            clbReactObjects = new System.Windows.Forms.CheckedListBox();
            lvMessaging = new System.Windows.Forms.ListView();
            Message = new System.Windows.Forms.ColumnHeader();
            lblMessages = new System.Windows.Forms.Label();
            btnOpenOutputDirectory = new System.Windows.Forms.Button();
            tabAbout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pbxLogo).BeginInit();
            tabOutput.SuspendLayout();
            tabExport.SuspendLayout();
            tabCsharp.SuspendLayout();
            panCSharpOptions.SuspendLayout();
            tabSql.SuspendLayout();
            tabServer.SuspendLayout();
            tabcontrolAutoCodeGen.SuspendLayout();
            tabWebService.SuspendLayout();
            tabReact.SuspendLayout();
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
            // tabExport
            // 
            tabExport.Controls.Add(btnToggleExportObjects);
            tabExport.Controls.Add(lblXml);
            tabExport.Controls.Add(clbExportOptionsTables);
            tabExport.Controls.Add(btnToggleExportSqlTables);
            tabExport.Controls.Add(lblXMLExportTables);
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
            btnToggleExportObjects.Click += btnToggleWebXmlObjects_Click;
            // 
            // lblXml
            // 
            resources.ApplyResources(lblXml, "lblXml");
            lblXml.Name = "lblXml";
            // 
            // clbExportOptionsTables
            // 
            resources.ApplyResources(clbExportOptionsTables, "clbExportOptionsTables");
            clbExportOptionsTables.CheckOnClick = true;
            clbExportOptionsTables.FormattingEnabled = true;
            clbExportOptionsTables.Name = "clbExportOptionsTables";
            clbExportOptionsTables.Sorted = true;
            clbExportOptionsTables.ItemCheck += clbExportOptionTables_ItemCheck;
            // 
            // btnToggleExportSqlTables
            // 
            resources.ApplyResources(btnToggleExportSqlTables, "btnToggleExportSqlTables");
            btnToggleExportSqlTables.Name = "btnToggleExportSqlTables";
            btnToggleExportSqlTables.UseVisualStyleBackColor = true;
            btnToggleExportSqlTables.Click += btnToggleWebXmlSqlTables_Click;
            // 
            // lblXMLExportTables
            // 
            resources.ApplyResources(lblXMLExportTables, "lblXMLExportTables");
            lblXMLExportTables.Name = "lblXMLExportTables";
            // 
            // clbExportSqlTables
            // 
            resources.ApplyResources(clbExportSqlTables, "clbExportSqlTables");
            clbExportSqlTables.CheckOnClick = true;
            clbExportSqlTables.FormattingEnabled = true;
            clbExportSqlTables.Name = "clbExportSqlTables";
            clbExportSqlTables.ItemCheck += clbExportSqlTables_ItemCheck;
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
            // btnToggleReactSqlTables
            // 
            resources.ApplyResources(btnToggleReactSqlTables, "btnToggleReactSqlTables");
            btnToggleReactSqlTables.Name = "btnToggleReactSqlTables";
            btnToggleReactSqlTables.UseVisualStyleBackColor = true;
            btnToggleReactSqlTables.Click += btnToggleAspSqlTables_Click;
            // 
            // tabSql
            // 
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
            tabcontrolAutoCodeGen.Controls.Add(tabWebService);
            tabcontrolAutoCodeGen.Controls.Add(tabReact);
            tabcontrolAutoCodeGen.Controls.Add(tabExport);
            tabcontrolAutoCodeGen.Controls.Add(tabOutput);
            tabcontrolAutoCodeGen.Controls.Add(tabAbout);
            tabcontrolAutoCodeGen.Name = "tabcontrolAutoCodeGen";
            tabcontrolAutoCodeGen.SelectedIndex = 0;
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
            // tabReact
            // 
            tabReact.Controls.Add(btnToggleReactSqlTables);
            tabReact.Controls.Add(lblASPTables);
            tabReact.Controls.Add(clbReactSqlTables);
            tabReact.Controls.Add(lblASPObjects);
            tabReact.Controls.Add(btnToggleASPObjects);
            tabReact.Controls.Add(clbReactObjects);
            resources.ApplyResources(tabReact, "tabReact");
            tabReact.Name = "tabReact";
            tabReact.UseVisualStyleBackColor = true;
            // 
            // lblASPTables
            // 
            resources.ApplyResources(lblASPTables, "lblASPTables");
            lblASPTables.Name = "lblASPTables";
            // 
            // clbReactSqlTables
            // 
            resources.ApplyResources(clbReactSqlTables, "clbReactSqlTables");
            clbReactSqlTables.CheckOnClick = true;
            clbReactSqlTables.FormattingEnabled = true;
            clbReactSqlTables.Name = "clbReactSqlTables";
            clbReactSqlTables.ItemCheck += clbReactSqlTables_ItemCheck;
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
            // clbReactObjects
            // 
            resources.ApplyResources(clbReactObjects, "clbReactObjects");
            clbReactObjects.CheckOnClick = true;
            clbReactObjects.FormattingEnabled = true;
            clbReactObjects.Name = "clbReactObjects";
            clbReactObjects.Sorted = true;
            clbReactObjects.ItemCheck += clbReactObjects_ItemCheck;
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
            tabExport.ResumeLayout(false);
            tabExport.PerformLayout();
            tabCsharp.ResumeLayout(false);
            tabCsharp.PerformLayout();
            panCSharpOptions.ResumeLayout(false);
            panCSharpOptions.PerformLayout();
            tabSql.ResumeLayout(false);
            tabSql.PerformLayout();
            tabServer.ResumeLayout(false);
            tabServer.PerformLayout();
            tabcontrolAutoCodeGen.ResumeLayout(false);
            tabWebService.ResumeLayout(false);
            tabWebService.PerformLayout();
            tabReact.ResumeLayout(false);
            tabReact.PerformLayout();
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
        private System.Windows.Forms.Button btnToggleExportSqlTables;
        private System.Windows.Forms.Button btnToggleCSharpObjects;
        private System.Windows.Forms.Button btnToggleCsharpSqlTables;
        private System.Windows.Forms.Button btnToggleASPObjects;
        private System.Windows.Forms.Button btnToggleReactSqlTables;
        private System.Windows.Forms.Button btnToggleOutput;

        private System.Windows.Forms.TabControl tabcontrolAutoCodeGen;
        private System.Windows.Forms.TabPage tabAbout;
        private System.Windows.Forms.TabPage tabWebService;
        private System.Windows.Forms.TabPage tabReact;
        private System.Windows.Forms.TabPage tabExport;
        private System.Windows.Forms.TabPage tabCsharp;
        private System.Windows.Forms.TabPage tabSql;
        private System.Windows.Forms.TabPage tabOutput;
        private System.Windows.Forms.TabPage tabServer;

        private System.Windows.Forms.CheckedListBox clbWebServiceObjects;
        private System.Windows.Forms.CheckedListBox clbReactSqlTables;
        private System.Windows.Forms.CheckedListBox clbWebServiceSqlTables;
        private System.Windows.Forms.CheckedListBox clbReactObjects;
        private System.Windows.Forms.CheckedListBox clbExportSqlTables;
        private System.Windows.Forms.CheckedListBox clbCsharpSqlTables;
        private System.Windows.Forms.CheckedListBox clbCsharpObjects;
        private System.Windows.Forms.CheckedListBox clbOutputOptions;
        private System.Windows.Forms.CheckedListBox clbTsqlSqlObjects;
        private System.Windows.Forms.CheckedListBox clbTsqlSqlTables;
        private System.Windows.Forms.LinkLabel lnkEmail;
        private System.Windows.Forms.ListView lvMessaging;
        private System.Windows.Forms.ColumnHeader Message;
        private System.Windows.Forms.Label lblMessages;
        private System.Windows.Forms.Panel panCSharpOptions;
        private System.Windows.Forms.ComboBox cboNamespaceIncludes;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnAddNamespace;
        private System.Windows.Forms.Button btnToggleWebServiceObjects;
        private System.Windows.Forms.Button btnToggleWebServicesSqlTables;
        private System.Windows.Forms.Label lblXml;
        private System.Windows.Forms.CheckedListBox clbExportOptionsTables;
        private System.Windows.Forms.Button btnToggleExportObjects;
        private System.Windows.Forms.Label lblCopyright;
        private System.Windows.Forms.Button btnOpenOutputDirectory;
    }
}