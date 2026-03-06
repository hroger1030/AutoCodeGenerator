namespace AutoCodeGen.Forms
{
    partial class FeatureGenerationList
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
            clbColumns = new System.Windows.Forms.CheckedListBox();
            checkedListBox1 = new System.Windows.Forms.CheckedListBox();
            button1 = new System.Windows.Forms.Button();
            button2 = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // clbColumns
            // 
            clbColumns.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            clbColumns.CheckOnClick = true;
            clbColumns.FormattingEnabled = true;
            clbColumns.Location = new System.Drawing.Point(13, 47);
            clbColumns.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            clbColumns.Name = "clbColumns";
            clbColumns.Size = new System.Drawing.Size(253, 292);
            clbColumns.TabIndex = 1;
            // 
            // checkedListBox1
            // 
            checkedListBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            checkedListBox1.CheckOnClick = true;
            checkedListBox1.FormattingEnabled = true;
            checkedListBox1.Location = new System.Drawing.Point(307, 47);
            checkedListBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkedListBox1.Name = "checkedListBox1";
            checkedListBox1.Size = new System.Drawing.Size(264, 292);
            checkedListBox1.TabIndex = 2;
            // 
            // button1
            // 
            button1.Location = new System.Drawing.Point(255, 12);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(75, 23);
            button1.TabIndex = 3;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            button2.Location = new System.Drawing.Point(416, 12);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(75, 23);
            button2.TabIndex = 4;
            button2.Text = "button2";
            button2.UseVisualStyleBackColor = true;
            // 
            // FeatureGenerationList
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(584, 361);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(checkedListBox1);
            Controls.Add(clbColumns);
            Name = "FeatureGenerationList";
            Text = "FeatureGenerationList";
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.CheckedListBox clbColumns;
        private System.Windows.Forms.CheckedListBox checkedListBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}