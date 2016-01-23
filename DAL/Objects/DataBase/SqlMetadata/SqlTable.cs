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
using System.Data;

namespace DAL.SqlMetadata
{
    public class SqlTable
    {
        #region Fields

            protected SqlDatabase _Database;
            protected string _Name;
            protected Dictionary<string,SqlColumn> _Columns;

        #endregion

        #region Properties

            public SqlDatabase Database
            {
                get { return _Database; }
                set { _Database = value; }
            }
            public string Name
            {
                get { return _Name; }
                set { _Name = value; }
            }
            public Dictionary<string,SqlColumn> Columns
            {
                get { return _Columns; }
                set { _Columns = value; }
            }

            // composite properties
            public Dictionary<string,SqlConstraint> TableConsraints
            {
                get 
                {
                    Dictionary<string,SqlConstraint> output = new Dictionary<string,SqlConstraint>();

                    if (_Database != null)
                    {
                        foreach (KeyValuePair<string,SqlConstraint> kvp in _Database.Constraints)
                        {
                            if (kvp.Value.PKTable == _Name || kvp.Value.FKTable == _Name)
                            {
                                if (!output.ContainsKey(kvp.Key))
                                    output.Add(kvp.Key,kvp.Value);
                            }
                        }
                    }

                    return output;
                }
            }
			public List<SqlColumn> PkList
			{
				get 
				{
					List<SqlColumn> output = new List<SqlColumn>();
 
					foreach (SqlColumn sql_column in _Columns.Values)
					{
						if (sql_column.IsPk)
							output.Add(sql_column);
					}

					return output;
				}
			}
       
        #endregion

        #region Methods

            public SqlTable()
            {
                Reset();
            }

            public SqlTable(SqlDatabase sql_database, string table_name)
            {
                _Database   = sql_database;
                _Name       = table_name;
                _Columns    = new Dictionary<string,SqlColumn>();
            }

            public void Reset()
            {
                _Database   = null;
                _Name       = string.Empty;
                _Columns    = new Dictionary<string,SqlColumn>();      
            }

            protected void GetColumnMetaData(DataTable dt)
            {
                _Columns.Clear();

                if (dt != null && dt.Rows.Count != 0 && dt.Columns.Count != 0)
                {
                    SqlColumn obj;

                    foreach (DataRow dr in dt.Rows)
                    {
                        // For some strange reason, if a column's type is nvarchar SQL2K
                        // will add an additional entry to the syscolumns table with the 
                        // type listed as a sysname. Since we don't want duplicat entries, omit.

                        //if ((string)dr["DataType"] == "sysname")
                        //    continue;

                        obj = new SqlColumn
                        (
                                this,
                                (string)dr["ColumnName"],
                                (string)dr["DataType"],
                                (int)dr["Length"],
                                (int)dr["Precision"],
                                (int)dr["Scale"],
                                (bool)dr["IsNullable"],
                                (bool)dr["IsPK"],
                                (bool)dr["IsIdentity"],
                                (int)dr["ColumnOrdinal"],
                                (string)dr["DefaultValue"]
                        );

                        _Columns.Add(obj.Name, obj);
                    }
                }
                else
                {
                    throw new Exception("Cannot retrieve metadata for table " + this._Name + ".");
                }
            }

        #endregion
    }
}
