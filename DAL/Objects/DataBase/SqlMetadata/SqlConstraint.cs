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
using System.Text;

namespace DAL.SqlMetadata
{
    public class SqlConstraint
    {
        #region Fields

            protected string _ConstraintName;
            protected string _FKTable;
            protected string _FKColumn;
            protected string _PKTable;
            protected string _PKColumn;

        #endregion

        #region Properties

            public string ConstraintName
            {
                get { return _ConstraintName; }
                set { _ConstraintName = value; }
            }
            public string FKTable
            {
                get { return _FKTable; }
                set { _FKTable = value; }
            }
            public string FKColumn
            {
                get { return _FKColumn; }
                set { _FKColumn = value; }
            }
            public string PKTable
            {
                get { return _PKTable; }
                set { _PKTable = value; }
            }
            public string PKColumn
            {
                get { return _PKColumn; }
                set { _PKColumn = value; }
            }

        #endregion

        #region Methods

            public SqlConstraint()
            {
                Reset();
            }

            public SqlConstraint(string constraint_name, string fk_table, string fk_column, string pk_table, string pk_column)
            {
                _ConstraintName     = constraint_name;
                _FKTable            = fk_table;
                _FKColumn           = fk_column;
                _PKTable            = pk_table;
                _PKColumn           = pk_column;
            }

            public void Reset()
            {
                _ConstraintName     = string.Empty;
                _FKTable            = string.Empty;
                _FKColumn           = string.Empty;
                _PKTable            = string.Empty;
                _PKColumn           = string.Empty;
            }

            public string GenerateSQLScript()
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine(" ALTER TABLE " + _FKTable);
                sb.AppendLine(" ADD CONSTRAINT " + _ConstraintName);
                sb.AppendLine(" FOREIGN KEY(" + _FKColumn + ")");
                sb.AppendLine(" REFERENCES " + _PKTable + "(" + _PKColumn + ");");

                return sb.ToString();
            }

            public void GenerateConstraintName()
            {
                _ConstraintName = "FK_" + _FKTable + "_" + _PKTable + "_" + this.GetHashCode().ToString();
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;

                if (this.GetType() != obj.GetType())
                    return false;

                SqlConstraint other = (SqlConstraint)obj;

                //if (this._ConstraintName != other._ConstraintName)
                //    return false;

                if (this._FKTable != other._FKTable)
                    return false;

                if (this._FKColumn != other._FKColumn)
                    return false;

                if (this._PKTable != other._PKTable)
                    return false;

                if (this._PKColumn != other._PKColumn)
                    return false;

                return true;
            }

            public override int GetHashCode()
            {
                int hash = 0;

                //hash ^= _ConstraintName.GetHashCode();
                hash ^= _FKTable.GetHashCode();
                hash ^= _FKColumn.GetHashCode();
                hash ^= _PKTable.GetHashCode();
                hash ^= _PKColumn.GetHashCode();

                return base.GetHashCode();
            }

            public override string ToString()
            {
                return "[" + _PKTable + "].[" + _PKColumn + "] = [" + _FKTable + "].[" + _FKColumn + "]";
            }

        #endregion
    }
}
