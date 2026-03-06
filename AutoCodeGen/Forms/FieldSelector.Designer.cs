namespace AutoCodeGen
{
    partial class frmFieldSelector
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmFieldSelector));
            clbColumns = new System.Windows.Forms.CheckedListBox();
            lblInstructions = new System.Windows.Forms.Label();
            btnDone = new System.Windows.Forms.Button();
            lblColumnList = new System.Windows.Forms.Label();
            btnClear = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // clbColumns
            // 
            clbColumns.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            clbColumns.CheckOnClick = true;
            clbColumns.FormattingEnabled = true;
            clbColumns.Location = new System.Drawing.Point(15, 64);
            clbColumns.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            clbColumns.Name = "clbColumns";
            clbColumns.Size = new System.Drawing.Size(230, 256);
            clbColumns.TabIndex = 0;
            clbColumns.ItemCheck += clbColumns_ItemCheck;
            // 
            // lblInstructions
            // 
            lblInstructions.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblInstructions.Location = new System.Drawing.Point(15, 13);
            lblInstructions.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblInstructions.Name = "lblInstructions";
            lblInstructions.Size = new System.Drawing.Size(428, 42);
            lblInstructions.TabIndex = 1;
            lblInstructions.Text = "Instructions Here";
            // 
            // btnDone
            // 
            btnDone.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnDone.Location = new System.Drawing.Point(356, 293);
            btnDone.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnDone.Name = "btnDone";
            btnDone.Size = new System.Drawing.Size(88, 27);
            btnDone.TabIndex = 2;
            btnDone.Text = "Done";
            btnDone.UseVisualStyleBackColor = true;
            btnDone.Click += btnDone_Click;
            // 
            // lblColumnList
            // 
            lblColumnList.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            lblColumnList.Location = new System.Drawing.Point(261, 64);
            lblColumnList.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblColumnList.Name = "lblColumnList";
            lblColumnList.Size = new System.Drawing.Size(182, 217);
            lblColumnList.TabIndex = 3;
            lblColumnList.Text = "Column List Here";
            // 
            // btnClear
            // 
            btnClear.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnClear.Location = new System.Drawing.Point(261, 293);
            btnClear.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnClear.Name = "btnClear";
            btnClear.Size = new System.Drawing.Size(88, 27);
            btnClear.TabIndex = 4;
            btnClear.Text = "Clear";
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += btnClear_Click;
            // 
            // frmFieldSelector
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(457, 330);
            Controls.Add(btnClear);
            Controls.Add(lblColumnList);
            Controls.Add(btnDone);
            Controls.Add(lblInstructions);
            Controls.Add(clbColumns);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MinimumSize = new System.Drawing.Size(464, 355);
            Name = "frmFieldSelector";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Sql Table Field Selector";
            FormClosing += FieldSelector_FormClosing;
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckedListBox clbColumns;
        private System.Windows.Forms.Label lblInstructions;
        private System.Windows.Forms.Button btnDone;
        private System.Windows.Forms.Label lblColumnList;
        private System.Windows.Forms.Button btnClear;
    }
}