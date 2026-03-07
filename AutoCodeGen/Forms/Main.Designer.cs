using System.Drawing;

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
            lblMessages = new System.Windows.Forms.Label();
            btnOpenOutputDirectory = new System.Windows.Forms.Button();
            rtbMessaging = new System.Windows.Forms.RichTextBox();
            btnCleanOutput = new System.Windows.Forms.Button();
            tabServer = new System.Windows.Forms.TabPage();
            btnSetDirectory = new System.Windows.Forms.Button();
            lblOutputPath = new System.Windows.Forms.Label();
            txtOutputPath = new System.Windows.Forms.TextBox();
            txtConn = new System.Windows.Forms.TextBox();
            btnUseDefaultConn = new System.Windows.Forms.Button();
            lblLogon = new System.Windows.Forms.Label();
            cmbDatabaseList = new System.Windows.Forms.ComboBox();
            lblDatabase = new System.Windows.Forms.Label();
            btnConnect = new System.Windows.Forms.Button();
            tabcontrolAutoCodeGen = new System.Windows.Forms.TabControl();
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
            // rtbMessaging
            // 
            resources.ApplyResources(rtbMessaging, "rtbMessaging");
            rtbMessaging.BackColor = SystemColors.Window;
            rtbMessaging.Name = "rtbMessaging";
            rtbMessaging.ReadOnly = true;
            // 
            // btnCleanOutput
            // 
            resources.ApplyResources(btnCleanOutput, "btnCleanOutput");
            btnCleanOutput.Name = "btnCleanOutput";
            btnCleanOutput.UseVisualStyleBackColor = true;
            btnCleanOutput.Click += btnCleanOutput_Click;
            // 
            // tabServer
            // 
            tabServer.Controls.Add(btnSetDirectory);
            tabServer.Controls.Add(lblOutputPath);
            tabServer.Controls.Add(txtOutputPath);
            tabServer.Controls.Add(txtConn);
            tabServer.Controls.Add(btnUseDefaultConn);
            tabServer.Controls.Add(lblLogon);
            tabServer.Controls.Add(cmbDatabaseList);
            tabServer.Controls.Add(lblDatabase);
            tabServer.Controls.Add(btnConnect);
            resources.ApplyResources(tabServer, "tabServer");
            tabServer.Name = "tabServer";
            tabServer.UseVisualStyleBackColor = true;
            // 
            // btnSetDirectory
            // 
            resources.ApplyResources(btnSetDirectory, "btnSetDirectory");
            btnSetDirectory.Name = "btnSetDirectory";
            btnSetDirectory.UseVisualStyleBackColor = true;
            btnSetDirectory.Click += btnSetDirectory_Click;
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
            // 
            // txtConn
            // 
            resources.ApplyResources(txtConn, "txtConn");
            txtConn.Name = "txtConn";
            // 
            // btnUseDefaultConn
            // 
            resources.ApplyResources(btnUseDefaultConn, "btnUseDefaultConn");
            btnUseDefaultConn.Name = "btnUseDefaultConn";
            btnUseDefaultConn.UseVisualStyleBackColor = true;
            btnUseDefaultConn.Click += btnUseDefaultConn_Click;
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
            tabcontrolAutoCodeGen.Name = "tabcontrolAutoCodeGen";
            tabcontrolAutoCodeGen.SelectedIndex = 0;
            // 
            // Main
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            Controls.Add(btnCleanOutput);
            Controls.Add(rtbMessaging);
            Controls.Add(btnOpenOutputDirectory);
            Controls.Add(lblMessages);
            Controls.Add(btnGenerateCode);
            Controls.Add(tabcontrolAutoCodeGen);
            Name = "Main";
            FormClosing += Main_FormClosing;
            tabServer.ResumeLayout(false);
            tabServer.PerformLayout();
            tabcontrolAutoCodeGen.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.Button btnGenerateCode;
        private System.Windows.Forms.Label lblMessages;
        private System.Windows.Forms.Button btnOpenOutputDirectory;
        private System.Windows.Forms.RichTextBox rtbMessaging;
        private System.Windows.Forms.Button btnCleanOutput;
        private System.Windows.Forms.TabPage tabServer;
        private System.Windows.Forms.Button btnSetDirectory;
        private System.Windows.Forms.Label lblOutputPath;
        private System.Windows.Forms.TextBox txtOutputPath;
        private System.Windows.Forms.TextBox txtConn;
        private System.Windows.Forms.Button btnUseDefaultConn;
        private System.Windows.Forms.Label lblLogon;
        private System.Windows.Forms.ComboBox cmbDatabaseList;
        private System.Windows.Forms.Label lblDatabase;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.TabControl tabcontrolAutoCodeGen;
    }
}