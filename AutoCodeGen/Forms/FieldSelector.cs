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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DAL.SqlMetadata;

namespace AutoCodeGen
{
    public partial class FieldSelector : Form
    {
        #region Constants

            protected const string s_SelectedItems = "Selected Columns:";

        #endregion

        #region Fields

            protected List<string> _SelectedColumns;
            protected int _MinSelections;
            protected int _MaxSelections;

        #endregion

        #region Methods

            public FieldSelector(List<string> selected_columns, SqlTable sql_table, string instructions) : this(selected_columns, sql_table, instructions, 1, -1) {}

            public FieldSelector(List<string> selected_columns, SqlTable sql_table, string instructions, int min_selections, int max_selections)
            {
                InitializeComponent();

               this.Size = new Size(Properties.Settings.Default.FieldSelectorWidth, Properties.Settings.Default.FieldSelectorHeight);

                _SelectedColumns                = selected_columns;
                _MinSelections                  = min_selections;
                _MaxSelections                  = max_selections;
                this.lblInstructions.Text       = instructions;

                this.ListSelectedColumns();
                this.BindColumnList(sql_table);

                this.btnDone.Enabled = ValidateSelections();
            }

            private void BindColumnList(SqlTable sql_table)
            {
                if (this.clbColumns.Items.Count != 0)
                    this.clbColumns.Items.Clear();

                foreach (var item in sql_table.Columns)
                    this.clbColumns.Items.AddRange(new object[] { item.Value.Name });
            }

            private void ListSelectedColumns()
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine(s_SelectedItems);

                foreach (var item in _SelectedColumns)
                    sb.AppendLine(item);

                this.lblColumnList.Text = sb.ToString();
            }

            private void btnDone_Click(object sender, EventArgs e)
            {
                this.Close();
            } 

            private void btnClear_Click(object sender, EventArgs e)
            {
                for (int i = 0; i < this.clbColumns.Items.Count; i++)
                    this.clbColumns.SetItemChecked(i, false);

                this.lblColumnList.Text = string.Empty;

                _SelectedColumns.Clear();

                ListSelectedColumns();
            }

            private void clbColumns_ItemCheck(object sender, ItemCheckEventArgs e)
            {
                string item = this.clbColumns.Items[e.Index].ToString();

                if (e.NewValue == CheckState.Checked)
                    _SelectedColumns.Add(item);
                else
                    _SelectedColumns.Remove(item);

                ListSelectedColumns();

                this.btnDone.Enabled = ValidateSelections();
            }

            private void FieldSelector_FormClosing(object sender, FormClosingEventArgs e)
            {
                Properties.Settings.Default.FieldSelectorHeight     = this.Size.Height;
                Properties.Settings.Default.FieldSelectorWidth      = this.Size.Width;

                Properties.Settings.Default.Save();
            }

            private bool ValidateSelections()
            {
                return (_SelectedColumns.Count >= _MinSelections || _MinSelections == -1) 
                    && (_SelectedColumns.Count <= _MaxSelections || _MaxSelections == -1);
            }

        #endregion
    }
}
