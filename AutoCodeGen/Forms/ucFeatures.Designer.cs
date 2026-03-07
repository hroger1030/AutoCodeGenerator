namespace AutoCodeGen
{
    partial class ucFeatures
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblTables = new System.Windows.Forms.Label();
            btnToggleFeatures = new System.Windows.Forms.Button();
            btnToggleTables = new System.Windows.Forms.Button();
            cblFeatures = new System.Windows.Forms.CheckedListBox();
            cblTables = new System.Windows.Forms.CheckedListBox();
            lblFeatures = new System.Windows.Forms.Label();
            lblDescription = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // lblTables
            // 
            lblTables.AutoSize = true;
            lblTables.Location = new System.Drawing.Point(18, 62);
            lblTables.Name = "lblTables";
            lblTables.Size = new System.Drawing.Size(90, 15);
            lblTables.TabIndex = 10;
            lblTables.Text = "Database Tables";
            // 
            // btnToggleFeatures
            // 
            btnToggleFeatures.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnToggleFeatures.Location = new System.Drawing.Point(484, 58);
            btnToggleFeatures.Name = "btnToggleFeatures";
            btnToggleFeatures.Size = new System.Drawing.Size(100, 23);
            btnToggleFeatures.TabIndex = 9;
            btnToggleFeatures.Text = "Toggle Features";
            btnToggleFeatures.UseVisualStyleBackColor = true;
            btnToggleFeatures.Click += btnToggleFeatures_Click;
            // 
            // btnToggleTables
            // 
            btnToggleTables.Location = new System.Drawing.Point(179, 58);
            btnToggleTables.Name = "btnToggleTables";
            btnToggleTables.Size = new System.Drawing.Size(100, 23);
            btnToggleTables.TabIndex = 8;
            btnToggleTables.Text = "Toggle Tables";
            btnToggleTables.UseVisualStyleBackColor = true;
            btnToggleTables.Click += btnToggleTables_Click;
            // 
            // cblFeatures
            // 
            cblFeatures.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            cblFeatures.CheckOnClick = true;
            cblFeatures.FormattingEnabled = true;
            cblFeatures.Location = new System.Drawing.Point(299, 87);
            cblFeatures.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cblFeatures.Name = "cblFeatures";
            cblFeatures.Size = new System.Drawing.Size(285, 292);
            cblFeatures.TabIndex = 7;
            // 
            // cblTables
            // 
            cblTables.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            cblTables.CheckOnClick = true;
            cblTables.FormattingEnabled = true;
            cblTables.Location = new System.Drawing.Point(18, 87);
            cblTables.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cblTables.Name = "cblTables";
            cblTables.Size = new System.Drawing.Size(261, 292);
            cblTables.TabIndex = 6;
            // 
            // lblFeatures
            // 
            lblFeatures.AutoSize = true;
            lblFeatures.Location = new System.Drawing.Point(299, 62);
            lblFeatures.Name = "lblFeatures";
            lblFeatures.Size = new System.Drawing.Size(51, 15);
            lblFeatures.TabIndex = 11;
            lblFeatures.Text = "Features";
            // 
            // lblDescription
            // 
            lblDescription.AutoSize = true;
            lblDescription.Location = new System.Drawing.Point(18, 12);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new System.Drawing.Size(103, 15);
            lblDescription.TabIndex = 12;
            lblDescription.Text = "(Description Here)";
            // 
            // ucFeatures
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(lblDescription);
            Controls.Add(lblFeatures);
            Controls.Add(lblTables);
            Controls.Add(btnToggleFeatures);
            Controls.Add(btnToggleTables);
            Controls.Add(cblFeatures);
            Controls.Add(cblTables);
            Name = "ucFeatures";
            Size = new System.Drawing.Size(600, 400);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTables;
        private System.Windows.Forms.Button btnToggleFeatures;
        private System.Windows.Forms.Button btnToggleTables;
        private System.Windows.Forms.CheckedListBox cblFeatures;
        private System.Windows.Forms.CheckedListBox cblTables;
        private System.Windows.Forms.Label lblFeatures;
        private System.Windows.Forms.Label lblDescription;
    }
}
