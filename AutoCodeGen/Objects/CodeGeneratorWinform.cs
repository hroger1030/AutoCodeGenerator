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

using System.Text;

using DAL.SqlMetadata;

namespace AutoCodeGenLibrary
{
    public class CodeGeneratorWinform : CodeGeneratorBase
    {
        public CodeGeneratorWinform() { }

        public OutputObject GenerateWinformEditCode(SqlTable sqlTable)
        {
            #region Sample Output
            //using System;
            //using System.Collections.Generic;
            //using System.ComponentModel;
            //using System.Data;
            //using System.Drawing;
            //using System.Linq;
            //using System.Text;
            //using System.Windows.Forms;

            //namespace MarvelSuperheroesEditor
            //{
            //    public partial class Form1 : Form
            //    {
            //        public Form1()
            //        {
            //            InitializeComponent();
            //        }
            //    }
            //}
            #endregion

            if (sqlTable == null)
                return null;

            string class_name = "frmEdit" + NameFormatter.ToCSharpPropertyName(sqlTable.Name);
            string dal_name = NameFormatter.ToCSharpClassName("Dal" + sqlTable.Name);

            OutputObject output = new OutputObject();
            output.Name = class_name + ".cs";
            output.Type = OutputObject.eObjectType.CSharp;

            var sb = new StringBuilder();

            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Configuration;");
            sb.AppendLine("using System.ComponentModel;");
            sb.AppendLine("using System.Data;");
            //sb.AppendLine("using System.Drawing;");
            //sb.AppendLine("using System.Linq;");
            //sb.AppendLine("using System.Text;");
            sb.AppendLine("using System.Windows.Forms;");
            sb.AppendLine();

            sb.AppendLine("using DAL;");
            sb.AppendLine("using " + NameFormatter.ToCSharpPropertyName(sqlTable.Database.Name) + ";");
            sb.AppendLine();

            sb.AppendLine("namespace " + NameFormatter.ToCSharpPropertyName(sqlTable.Database.Name) + "Editor");
            sb.AppendLine("{");

            sb.AppendLine(AddTabs(1) + "public partial class " + class_name + " : Form");
            sb.AppendLine(AddTabs(1) + "{");

            #region Fields
            sb.AppendLine(AddTabs(2) + "#region Fields");
            sb.AppendLine();
            sb.AppendLine(AddTabs(3) + "protected string _SQLConnection = ConfigurationManager.ConnectionStrings[\"SQLConnection\"].ConnectionString;");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#endregion");
            sb.AppendLine();
            #endregion

            #region Methods
            sb.AppendLine(AddTabs(2) + "#region Methods");
            sb.AppendLine();

            sb.AppendLine(AddTabs(3) + "public " + class_name + "()");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "InitializeComponent();");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "LoadPopulateItemList();");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine();

            sb.AppendLine(AddTabs(3) + "private bool LoadPopulateItemList()");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "this.Cursor = Cursors.WaitCursor;");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "try");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "" + dal_name + " dal = new " + dal_name + "(_SQLConnection);");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "if (dal.LoadAllFromDb())");
            sb.AppendLine(AddTabs(5) + "{");
            sb.AppendLine(AddTabs(6) + "if (dal.Collection != null && dal.Collection.Count != 0)");
            sb.AppendLine(AddTabs(6) + "{");
            sb.AppendLine(AddTabs(7) + "cboItemList.BeginUpdate();");
            sb.AppendLine(AddTabs(7) + "cboItemList.ValueMember     = \"" + FindIdField(sqlTable) + "\";");
            sb.AppendLine(AddTabs(7) + "cboItemList.DisplayMember   = \"" + FindNameField(sqlTable) + "\";");
            sb.AppendLine(AddTabs(7) + "cboItemList.DataSource      = dal.Collection;");
            sb.AppendLine(AddTabs(7) + "cboItemList.EndUpdate();");
            sb.AppendLine(AddTabs(6) + "}");
            sb.AppendLine(AddTabs(5) + "}");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(4) + "catch");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "return false;");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "this.Cursor = Cursors.Default;");
            sb.AppendLine(AddTabs(4) + "return true;");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine();

            sb.AppendLine(AddTabs(2) + "#endregion");
            sb.AppendLine();
            #endregion

            #region Events
            sb.AppendLine(AddTabs(2) + "#region Events");
            sb.AppendLine();

            sb.AppendLine(AddTabs(3) + "private void btnCancel_Click(object sender, EventArgs e)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine(AddTabs(3) + "private void btnSave_Click(object sender, EventArgs e)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine(AddTabs(3) + "private void cboItemList_SelectedIndexChanged(object sender, EventArgs e)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(3) + "}");

            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#endregion");
            #endregion

            sb.AppendLine(AddTabs(1) + "}");
            sb.AppendLine("}");

            output.Body = sb.ToString();
            return output;
        }

        public OutputObject GenerateWinformEditCodeDesigner(SqlTable sqlTable)
        {
            #region Code Output
            //namespace MarvelSuperheroesEditor
            //{
            //    partial class Form1
            //    {
            //        /// <summary>
            //        /// Required designer variable.
            //        /// </summary>
            //        private System.ComponentModel.IContainer components = null;

            //        /// <summary>
            //        /// Clean up any resources being used.
            //        /// </summary>
            //        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
            //        protected override void Dispose(bool disposing)
            //        {
            //            if (disposing && (components != null))
            //            {
            //                components.Dispose();
            //            }
            //            base.Dispose(disposing);
            //        }

            //        #region Windows Form Designer generated code

            //        /// <summary>
            //        /// Required method for Designer support - do not modify
            //        /// the contents of this method with the code editor.
            //        /// </summary>
            //        private void InitializeComponent()
            //        {
            //            this.btnSave = new System.Windows.Forms.Button();
            //            this.btnCancel = new System.Windows.Forms.Button();
            //            this.label1 = new System.Windows.Forms.Label();
            //            this.textBox1 = new System.Windows.Forms.TextBox();
            //            this.SuspendLayout();
            //            // 
            //            // btnSave
            //            // 
            //            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            //            this.btnSave.Location = new System.Drawing.Point(232, 36);
            //            this.btnSave.Name = "btnSave";
            //            this.btnSave.Size = new System.Drawing.Size(75, 23);
            //            this.btnSave.TabIndex = 0;
            //            this.btnSave.Text = "Save";
            //            this.btnSave.UseVisualStyleBackColor = true;
            //            // 
            //            // btnCancel
            //            // 
            //            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            //            this.btnCancel.Location = new System.Drawing.Point(151, 36);
            //            this.btnCancel.Name = "btnCancel";
            //            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            //            this.btnCancel.TabIndex = 1;
            //            this.btnCancel.Text = "Cancel";
            //            this.btnCancel.UseVisualStyleBackColor = true;
            //            // 
            //            // label1
            //            // 
            //            this.label1.AutoSize = true;
            //            this.label1.Location = new System.Drawing.Point(12, 9);
            //            this.label1.Name = "label1";
            //            this.label1.Size = new System.Drawing.Size(35, 13);
            //            this.label1.TabIndex = 2;
            //            this.label1.Text = "label1";
            //            // 
            //            // textBox1
            //            // 
            //            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            //            | System.Windows.Forms.AnchorStyles.Right)));
            //            this.textBox1.Location = new System.Drawing.Point(118, 6);
            //            this.textBox1.Name = "textBox1";
            //            this.textBox1.Size = new System.Drawing.Size(189, 20);
            //            this.textBox1.TabIndex = 3;
            //            // 
            //            // Form1
            //            // 
            //            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            //            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            //            this.ClientSize = new System.Drawing.Size(319, 71);
            //            this.Controls.Add(this.textBox1);
            //            this.Controls.Add(this.label1);
            //            this.Controls.Add(this.btnCancel);
            //            this.Controls.Add(this.btnSave);
            //            this.Name = "Form1";
            //            this.Text = "Form1";
            //            this.ResumeLayout(false);
            //            this.PerformLayout();
            //        }

            //        #endregion

            //        private System.Windows.Forms.Button btnSave;
            //        private System.Windows.Forms.Button btnCancel;
            //        private System.Windows.Forms.Label label1;
            //        private System.Windows.Forms.TextBox textBox1;
            //    }
            //}
            #endregion

            if (sqlTable == null)
                return null;

            string class_name = "frmEdit" + NameFormatter.ToCSharpPropertyName(sqlTable.Name);

            OutputObject output = new OutputObject();
            output.Name = class_name + ".Designer.cs";
            output.Type = OutputObject.eObjectType.CSharp;

            int total_form_height = 56 + (26 * sqlTable.Columns.Values.Count);

            var sb = new StringBuilder();

            sb.AppendLine("namespace " + NameFormatter.ToCSharpPropertyName(sqlTable.Database.Name) + "Editor");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "partial class " + class_name);
            sb.AppendLine(AddTabs(1) + "{");
            sb.AppendLine(AddTabs(2) + "/// <summary>");
            sb.AppendLine(AddTabs(2) + "/// Required designer variable.");
            sb.AppendLine(AddTabs(2) + "/// </summary>");
            sb.AppendLine(AddTabs(2) + "private System.ComponentModel.IContainer components = null;");
            sb.AppendLine();

            sb.AppendLine(AddTabs(2) + "/// <summary>");
            sb.AppendLine(AddTabs(2) + "/// Clean up any resources being used.");
            sb.AppendLine(AddTabs(2) + "/// </summary>");
            sb.AppendLine(AddTabs(2) + "/// <param name=\"disposing\">true if managed resources should be disposed; otherwise, false.</param>");
            sb.AppendLine(AddTabs(2) + "protected override void Dispose(bool disposing)");
            sb.AppendLine(AddTabs(2) + "{");
            sb.AppendLine(AddTabs(3) + "if (disposing && (components != null))");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "components.Dispose();");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine(AddTabs(3) + " base.Dispose(disposing);");
            sb.AppendLine(AddTabs(2) + "}");
            sb.AppendLine();

            sb.AppendLine(AddTabs(2) + "#region Windows Form Designer generated code");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "/// <summary>");
            sb.AppendLine(AddTabs(2) + "/// Required method for Designer support - do not modify");
            sb.AppendLine(AddTabs(2) + "/// the contents of this method with the code editor.");
            sb.AppendLine(AddTabs(2) + "/// </summary>");
            sb.AppendLine(AddTabs(2) + "private void InitializeComponent()");
            sb.AppendLine(AddTabs(2) + "{");
            sb.AppendLine(AddTabs(3) + "this.btnSave = new System.Windows.Forms.Button();");
            sb.AppendLine(AddTabs(3) + "this.btnCancel = new System.Windows.Forms.Button();");
            sb.AppendLine(AddTabs(3) + "this.cboItemList = new System.Windows.Forms.ComboBox();");
            sb.AppendLine();

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                sb.AppendLine(AddTabs(3) + "this.lbl" + sql_column.Name + " = new System.Windows.Forms.Label();");
                sb.AppendLine(AddTabs(3) + "this.txt" + sql_column.Name + " = new System.Windows.Forms.TextBox();");
                sb.AppendLine();
            }

            sb.AppendLine(AddTabs(3) + "this.SuspendLayout();");
            sb.AppendLine(AddTabs(3) + "//");
            sb.AppendLine(AddTabs(3) + "// btnSave");
            sb.AppendLine(AddTabs(3) + "//");
            sb.AppendLine(AddTabs(3) + "this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));");
            sb.AppendLine(AddTabs(3) + "this.btnSave.Location = new System.Drawing.Point(232," + (total_form_height - 26).ToString() + ");");
            sb.AppendLine(AddTabs(3) + "this.btnSave.Name = \"btnSave\";");
            sb.AppendLine(AddTabs(3) + "this.btnSave.Size = new System.Drawing.Size(75, 23);");
            sb.AppendLine(AddTabs(3) + "this.btnSave.TabIndex = 0;");
            sb.AppendLine(AddTabs(3) + "this.btnSave.Text = \"Save\";");
            sb.AppendLine(AddTabs(3) + "this.btnSave.UseVisualStyleBackColor = true;");
            sb.AppendLine(AddTabs(3) + "this.btnSave.Click += new System.EventHandler(this.btnSave_Click);");
            sb.AppendLine(AddTabs(3) + "//");
            sb.AppendLine(AddTabs(3) + "// btnCancel");
            sb.AppendLine(AddTabs(3) + "//");
            sb.AppendLine(AddTabs(3) + "this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));");
            sb.AppendLine(AddTabs(3) + "this.btnCancel.Location = new System.Drawing.Point(151," + (total_form_height - 26).ToString() + ");");
            sb.AppendLine(AddTabs(3) + "this.btnCancel.Name = \"btnCancel\";");
            sb.AppendLine(AddTabs(3) + "this.btnCancel.Size = new System.Drawing.Size(75, 23);");
            sb.AppendLine(AddTabs(3) + "this.btnCancel.TabIndex = 1;");
            sb.AppendLine(AddTabs(3) + "this.btnCancel.Text = \"Cancel\";");
            sb.AppendLine(AddTabs(3) + "this.btnCancel.UseVisualStyleBackColor = true;");
            sb.AppendLine(AddTabs(3) + "this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);");
            sb.AppendLine(AddTabs(3) + "//");
            sb.AppendLine(AddTabs(3) + "// cboItemList");
            sb.AppendLine(AddTabs(3) + "//");
            sb.AppendLine(AddTabs(3) + "this.cboItemList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));");
            sb.AppendLine(AddTabs(3) + "this.cboItemList.FormattingEnabled = true;");
            sb.AppendLine(AddTabs(3) + "this.cboItemList.Location = new System.Drawing.Point(12, 5);");
            sb.AppendLine(AddTabs(3) + "this.cboItemList.Name = \"cboItemList\";");
            sb.AppendLine(AddTabs(3) + "this.cboItemList.Size = new System.Drawing.Size(295, 21);");
            sb.AppendLine(AddTabs(3) + "this.cboItemList.TabIndex = 2;");
            sb.AppendLine(AddTabs(3) + "this.cboItemList.SelectedIndexChanged += new System.EventHandler(this.cboItemList_SelectedIndexChanged);");

            int height = 26;
            int tab_index = 3;

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                string label_name = "lbl" + NameFormatter.ToCSharpPropertyName(sql_column.Name);
                string textbox_name = "txt" + NameFormatter.ToCSharpPropertyName(sql_column.Name);

                sb.AppendLine(AddTabs(3) + "//");
                sb.AppendLine(AddTabs(3) + "// " + label_name);
                sb.AppendLine(AddTabs(3) + "//");
                sb.AppendLine(AddTabs(3) + "this." + label_name + ".AutoSize = true;");
                sb.AppendLine(AddTabs(3) + "this." + label_name + ".Location = new System.Drawing.Point(12," + (height + 9).ToString() + ");");
                sb.AppendLine(AddTabs(3) + "this." + label_name + ".Name = \"" + label_name + "\";");
                sb.AppendLine(AddTabs(3) + "this." + label_name + ".Size = new System.Drawing.Size(35, 13);");
                sb.AppendLine(AddTabs(3) + "this." + label_name + ".TabIndex = " + tab_index.ToString() + ";");
                sb.AppendLine(AddTabs(3) + "this." + label_name + ".Text = \"" + NameFormatter.ToFriendlyName(sql_column.Name) + "\";");

                tab_index++;

                sb.AppendLine(AddTabs(3) + "//");
                sb.AppendLine(AddTabs(3) + "// " + textbox_name);
                sb.AppendLine(AddTabs(3) + "//");
                sb.AppendLine(AddTabs(3) + "this." + textbox_name + ".Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));");
                sb.AppendLine(AddTabs(3) + "this." + textbox_name + ".Location = new System.Drawing.Point(118," + (height + 6).ToString() + ");");
                sb.AppendLine(AddTabs(3) + "this." + textbox_name + ".Name = \"" + textbox_name + "\";");
                sb.AppendLine(AddTabs(3) + "this." + textbox_name + ".Size = new System.Drawing.Size(189, 20);");
                sb.AppendLine(AddTabs(3) + "this." + textbox_name + ".TabIndex = " + tab_index.ToString() + ";");
                //sb.AppendLine(AddTabs(3) + "this." + textbox_name + ".MaxLength = " + sql_column.Length + ";");

                tab_index++;
                height += 26;
            }

            sb.AppendLine(AddTabs(3) + "//");
            sb.AppendLine(AddTabs(3) + "// " + class_name);
            sb.AppendLine(AddTabs(3) + "//");
            sb.AppendLine(AddTabs(3) + "this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);");
            sb.AppendLine(AddTabs(3) + "this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;");
            sb.AppendLine(AddTabs(3) + "this.ClientSize = new System.Drawing.Size(319," + total_form_height.ToString() + ");");

            sb.AppendLine(AddTabs(3) + "this.Controls.Add(this.btnCancel);");
            sb.AppendLine(AddTabs(3) + "this.Controls.Add(this.btnSave);");
            sb.AppendLine(AddTabs(3) + "this.Controls.Add(this.cboItemList);");
            sb.AppendLine();

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                sb.AppendLine(AddTabs(3) + "this.Controls.Add(this.txt" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ");");
                sb.AppendLine(AddTabs(3) + "this.Controls.Add(this.lbl" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ");");
                sb.AppendLine();
            }

            sb.AppendLine(AddTabs(3) + "this.MinimumSize = new System.Drawing.Size(319," + total_form_height.ToString() + ");");
            sb.AppendLine(AddTabs(3) + "this.Name = \"" + class_name + "\";");
            sb.AppendLine(AddTabs(3) + "this.Text = \"" + NameFormatter.ToCSharpPropertyName(sqlTable.Name) + "\";");
            sb.AppendLine();

            sb.AppendLine(AddTabs(3) + "this.ResumeLayout(false);");
            sb.AppendLine(AddTabs(3) + "this.PerformLayout();");

            sb.AppendLine(AddTabs(2) + "}");
            sb.AppendLine();

            sb.AppendLine(AddTabs(2) + "#endregion");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "private System.Windows.Forms.Button btnSave;");
            sb.AppendLine(AddTabs(2) + "private System.Windows.Forms.Button btnCancel;");
            sb.AppendLine(AddTabs(2) + "private System.Windows.Forms.ComboBox cboItemList;");
            sb.AppendLine();

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                sb.AppendLine(AddTabs(2) + "private System.Windows.Forms.Label lbl" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ";");
                sb.AppendLine(AddTabs(2) + "private System.Windows.Forms.TextBox txt" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ";");
                sb.AppendLine();
            }

            sb.AppendLine(AddTabs(2) + "}");
            sb.AppendLine(AddTabs(1) + "}");

            output.Body = sb.ToString();
            return output;
        }

        public OutputObject GenerateWinformViewCode(SqlTable sqlTable)
        {
            return null;
        }

        public OutputObject GenerateWinformViewCodeDesigner(SqlTable sqlTable)
        {
            return null;
        }

        public OutputObject GenerateWinformMainCode(SqlTable sqlTable)
        {
            return null;
        }

        public OutputObject GenerateWinformMainCodeDesigner(SqlTable sqlTable)
        {
            return null;
        }
    }
}