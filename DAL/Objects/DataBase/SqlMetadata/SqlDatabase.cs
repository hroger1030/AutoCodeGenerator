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
using System.Text;

namespace DAL.SqlMetadata
{
    public class SqlDatabase
    {
        #region Constants

            protected static string DEFAULT_CONNECTION_STRING = "Data Source=Localhost;Initial Catalog=Master;Integrated Security=SSPI;Connect Timeout=1;";
        
        #endregion
        
        #region Fields

            protected string _Name;
            protected Dictionary<string,SqlTable> _Tables;
            protected Dictionary<string,SqlScript> _StoredProcedures;
            protected Dictionary<string,SqlScript> _Functions;
            protected Dictionary<string,SqlConstraint> _Constraints;
            protected string _ConnectionString; 
            protected List<Exception> _ErrorList;

        #endregion

        #region Properties

            public string Name
            {
                get { return _Name; }
            }
            public Dictionary<string,SqlTable> Tables
            {
                get { return _Tables; }
                set { _Tables = value; }
            }
            public Dictionary<string,SqlScript> StoredProcedures
            {
                get { return _StoredProcedures; }
                set { _StoredProcedures = value; }
            }
            public Dictionary<string,SqlScript> Functions
            {
                get { return _Functions; }
                set { _Functions = value; }
            }
            public Dictionary<string,SqlConstraint> Constraints
            {
                get { return _Constraints; }
                set { _Constraints = value; }
            }
            public string ConnectionString
            {
                get { return _ConnectionString; }
                set { _ConnectionString = value; }
            }
            public List<Exception> ErrorList
            {
                get { return _ErrorList; }
                set { _ErrorList = value; }
            }
            public string FormattedDatabaseName
            {
                get { return "[" + _Name + "]"; }
            }

        #endregion

        #region Methods

            public SqlDatabase()
            {
                Reset();
            }

            public void Reset()
            {
                _Name               = string.Empty;
                _Tables             = new Dictionary<string,SqlTable>();
                _StoredProcedures   = new Dictionary<string,SqlScript>();
                _Functions          = new Dictionary<string,SqlScript>();
                _Constraints        = new Dictionary<string,SqlConstraint>();
                _ConnectionString   = string.Empty;
                _ErrorList          = new List<Exception>();
            }

            public bool LoadDatabaseMetadata(string database_name, string connection_string)
            {
                if (string.IsNullOrEmpty(database_name))
                    throw new ArgumentException("Database name is null or empty");

                Reset();

                _Name = database_name;
                _ConnectionString = connection_string;

                // load and parse out table data
                try
                {
                    string sql_query = GetTableData();

                    DataTable dt = Database.ExecuteQuery(sql_query, null, _ConnectionString);

                    if (dt != null && dt.Rows.Count != 0 && dt.Columns.Count != 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            string table_name   = (string)dr["TableName"];
                            string column_name  = (string)dr["ColumnName"];

                            if (!_Tables.ContainsKey(table_name))
                            {
                                SqlTable sql_table = new SqlTable(this,table_name);
                                _Tables.Add(table_name, sql_table);
                            }

                            SqlColumn sql_column = new SqlColumn();

                            sql_column.Table            = _Tables[table_name];
                            sql_column.Name             = (string)dr["ColumnName"];
                            sql_column.DataType         = (string)dr["DataType"];
                            sql_column.Length           = Convert.ToInt32(dr["Length"]);
                            sql_column.Precision        = Convert.ToInt32(dr["Precision"]);
                            sql_column.IsNullable       = Convert.ToBoolean(dr["IsNullable"]);
                            sql_column.IsPk             = Convert.ToBoolean(dr["IsPK"]);
                            sql_column.IsIdentity       = Convert.ToBoolean(dr["IsIdentity"]);
                            sql_column.ColumnOrdinal    = Convert.ToInt32(dr["ColumnOrdinal"]);

                            if (_Tables[table_name].Columns.ContainsKey(column_name))
                                throw new Exception(string.Format("Column {0} already exists in table {1}.", column_name, _Tables[table_name]));
                            else
                                _Tables[table_name].Columns.Add(column_name, sql_column);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _ErrorList.Add(ex);
                }

                // get SP
                try
                {
                    string sql_query = GetStoredProcedures();

                    DataTable dt = Database.ExecuteQuery(sql_query, null, _ConnectionString);

                    if (dt != null && dt.Rows.Count != 0 && dt.Columns.Count != 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            SqlScript sql_script = new SqlScript();

                            sql_script.Name     = (string)dr["Name"];
                            sql_script.Body     = (string)dr["Body"];

                            if (_StoredProcedures.ContainsKey(sql_script.Name))
                                _StoredProcedures[sql_script.Name].Body += sql_script.Body;
                            else
                                _StoredProcedures.Add(sql_script.Name, sql_script);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _ErrorList.Add(ex);
                }

                // get functions
                try
                {
                    string sql_query = GetFunctions();

                    DataTable dt = Database.ExecuteQuery(sql_query, null, _ConnectionString);

                    if (dt != null && dt.Rows.Count != 0 && dt.Columns.Count != 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            SqlScript sql_script = new SqlScript();

                            sql_script.Name     = (string)dr["Name"];
                            sql_script.Body     = (string)dr["Body"];

                            if (_Functions.ContainsKey(sql_script.Name))
                                _Functions[sql_script.Name].Body += sql_script.Body;
                            else
                                _Functions.Add(sql_script.Name, sql_script);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _ErrorList.Add(ex);
                }

                // get constraints
                try
                {
                    string sql_query = GetConstraints();

                    DataTable dt = Database.ExecuteQuery(sql_query, null, _ConnectionString);

                    if (dt != null && dt.Rows.Count != 0 && dt.Columns.Count != 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            SqlConstraint sql_constraint = new SqlConstraint();

                            sql_constraint.ConstraintName   = (string)dr["ConstraintName"];
                            sql_constraint.FKTable          = (string)dr["FKTable"];
                            sql_constraint.FKColumn         = (string)dr["FKColumn"];
                            sql_constraint.PKTable          = (string)dr["PKTable"];
                            sql_constraint.PKColumn         = (string)dr["PKColumn"];

                            if (_Constraints.ContainsKey(sql_constraint.ConstraintName))
                                throw new Exception(string.Format("Constraint {0} already exists.", sql_constraint.ConstraintName));
                            else
                                _Constraints.Add(sql_constraint.ConstraintName, sql_constraint);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _ErrorList.Add(ex);
                }

                // load default values
                try
                {
                    string sql_query = GetDefaultValues();

                    DataTable dt = Database.ExecuteQuery(sql_query, null, _ConnectionString);

                    if (dt != null && dt.Rows.Count != 0 && dt.Columns.Count != 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            if (_Tables.ContainsKey((string)dr["TableName"]))
                                if (_Tables[(string)dr["TableName"]].Columns.ContainsKey((string)dr["ColumnName"]))
                                    _Tables[(string)dr["TableName"]].Columns[(string)dr["ColumnName"]].DefaultValue = RemoveWrappingCharacters((string)dr["DefaultValue"]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _ErrorList.Add(ex);
                }

                return _ErrorList.Count == 0;
            }

            /// <summary>
            /// Generates SQL procedure to pull db info out of a table.
            /// Updated for SQL 2008, but several types are not supported:
            /// sysname, timestamp, hierarchyid, geometry, geography
            /// </summary>
            protected string GetTableData()
            {
                /*
                USE [<db>] 
               
                SELECT	sys.Objects.[Name]				            AS [TableName],
		                sys.columns.[Name]				            AS [ColumnName],
		                sys.types.[name]				            AS [DataType],
		                sys.columns.[max_length]		            AS [Length],
		                sys.columns.[precision]			            AS [Precision],
						sys.columns.[scale]				            AS [Scale],	 
		                sys.columns.[is_nullable]		            AS [IsNullable],
		                CAST(ISNULL(PrimaryKeys.IsPK,0) AS BIT)     AS [IsPK],
		                sys.columns.[is_identity]		            AS [IsIdentity],
		                sys.columns.column_id			            AS [ColumnOrdinal]
                		
                FROM	sys.objects 
		                INNER JOIN sys.columns ON sys.objects.object_id = sys.columns.object_id
		                INNER JOIN sys.types ON sys.columns.system_type_id = sys.types.system_type_id
		                LEFT JOIN
		                ( 
			                SELECT 	DISTINCT C.[TABLE_NAME]		AS [TableName],
					                K.[COLUMN_NAME]				AS [ColumnName],
					                1							AS [IsPK]				
                			
			                FROM 	INFORMATION_SCHEMA.KEY_COLUMN_USAGE K
					                INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS C ON K.TABLE_NAME = C.TABLE_NAME
			                WHERE	C.CONSTRAINT_TYPE = 'PRIMARY KEY'
		                ) PrimaryKeys ON PrimaryKeys.[TableName] = sys.Objects.[Name] AND PrimaryKeys.[ColumnName] = sys.columns.[Name]
                		
                WHERE	sys.objects.type = 'U'
                AND     sys.types.[name] NOT IN ('sysname','timestamp','hierarchyid','geometry','geography')
                AND     sys.types.is_user_defined = 0

                ORDER	BY sys.Objects.name,sys.columns.column_id
                */

                StringBuilder sb = new StringBuilder();

                 sb.AppendLine(" USE [" + _Name +"]");
                 sb.AppendLine(" SELECT sys.Objects.[Name] AS [TableName],");
                 sb.AppendLine(" sys.columns.[Name] AS [ColumnName],");
                 sb.AppendLine(" sys.types.[name] AS [DataType],");
                 sb.AppendLine(" sys.columns.[max_length] AS [Length],");
                 sb.AppendLine(" sys.columns.[precision] AS [Precision],");
                 sb.AppendLine(" sys.columns.[scale] AS [Scale],");
                 sb.AppendLine(" sys.columns.[is_nullable] AS [IsNullable],");
                 sb.AppendLine(" CAST(ISNULL(PrimaryKeys.IsPK,0) AS BIT) AS [IsPK],");
                 sb.AppendLine(" sys.columns.[is_identity] AS [IsIdentity],");
                 sb.AppendLine(" sys.columns.column_id AS [ColumnOrdinal]");
                 sb.AppendLine(" FROM sys.objects ");
                 sb.AppendLine(" INNER JOIN sys.columns ON sys.objects.object_id = sys.columns.object_id");
                 sb.AppendLine(" INNER JOIN sys.types ON sys.columns.system_type_id = sys.types.system_type_id");
                 sb.AppendLine(" LEFT JOIN");
                 sb.AppendLine(" ( ");
                 sb.AppendLine(" SELECT DISTINCT C.[TABLE_NAME] AS [TableName],");
                 sb.AppendLine(" K.[COLUMN_NAME] AS [ColumnName],");
                 sb.AppendLine(" 1 AS [IsPK] "); 
                 sb.AppendLine(" FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE K");
                 sb.AppendLine(" INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS C ON K.TABLE_NAME = C.TABLE_NAME");
                 sb.AppendLine(" WHERE C.CONSTRAINT_TYPE = 'PRIMARY KEY'");
                 sb.AppendLine(" ) PrimaryKeys ON PrimaryKeys.[TableName] = sys.Objects.[Name] AND PrimaryKeys.[ColumnName] = sys.columns.[Name]");
                 sb.AppendLine(" WHERE sys.objects.type = 'U'");
                 sb.AppendLine(" AND sys.types.[name] NOT IN ('sysname','timestamp','hierarchyid','geometry','geography')");
                 sb.AppendLine(" AND sys.types.is_user_defined = 0");
                 sb.AppendLine(" ORDER BY sys.Objects.name,sys.columns.column_id");

                return sb.ToString();
            }

            protected string GetStoredProcedures()
            {
                /*
                USE [<db>] 

                SELECT	sys.objects.name	AS [Name],
		                syscomments.text	AS [Body] 
                FROM	sys.objects
		                INNER JOIN syscomments ON sys.objects.object_id = syscomments.id
                WHERE	sys.objects.type = 'p'
                AND		sys.objects.is_ms_shipped = 0
                ORDER	BY sys.objects.name
                 */

                StringBuilder sb = new StringBuilder();

                sb.Append(" USE [" + _Name +"]");
                sb.Append(" SELECT sys.objects.name	AS [Name],");
		        sb.Append(" syscomments.text AS [Body]");
                sb.Append(" FROM sys.objects");
		        sb.Append(" INNER JOIN syscomments ON sys.objects.object_id = syscomments.id");
                sb.Append(" WHERE sys.objects.type = 'p'");
                sb.Append(" AND sys.objects.is_ms_shipped = 0");
                sb.Append(" ORDER BY sys.objects.name, syscomments.colid");

                return sb.ToString();
            }

            protected string GetFunctions()
            {
                /*
                USE [<db>] 
                 
                SELECT	sys.objects.name	AS [Name],
		                syscomments.text	AS [Body] 
                FROM	sys.objects
		                INNER JOIN syscomments ON sys.objects.object_id = syscomments.id
                WHERE	sys.objects.type = 'fn'
                AND		sys.objects.is_ms_shipped = 0
                ORDER	BY sys.objects.name
                */
                
                StringBuilder sb = new StringBuilder();

                sb.Append(" USE [" + _Name +"]");
                sb.Append(" SELECT sys.objects.name AS [Name],");
		        sb.Append(" syscomments.text AS [Body]");
                sb.Append(" FROM sys.objects");
		        sb.Append(" INNER JOIN syscomments ON sys.objects.object_id = syscomments.id");
                sb.Append(" WHERE sys.objects.type = 'fn'");
                sb.Append(" AND sys.objects.is_ms_shipped = 0");
                sb.Append(" ORDER BY sys.objects.name, syscomments.colid");

                return sb.ToString();
            }

            protected string GetConstraints()
            {
                /*
                USE [<db>] 
                
                SELECT	C.CONSTRAINT_NAME	AS [ConstraintName],
		                FK.TABLE_NAME		AS FKTable,
		                CU.COLUMN_NAME		AS FKColumn,
		                PK.TABLE_NAME		AS PKTable,
		                PT.COLUMN_NAME		AS PKColumn

                FROM	INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS C
		                INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS FK ON C.CONSTRAINT_NAME = FK.CONSTRAINT_NAME
		                INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS PK ON C.UNIQUE_CONSTRAINT_NAME = PK.CONSTRAINT_NAME
		                INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE CU ON C.CONSTRAINT_NAME = CU.CONSTRAINT_NAME
		                INNER JOIN 
		                (
			                SELECT	i1.TABLE_NAME, 
					                i2.COLUMN_NAME
			                FROM	INFORMATION_SCHEMA.TABLE_CONSTRAINTS i1
					                INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE i2 ON i1.CONSTRAINT_NAME = i2.CONSTRAINT_NAME
			                WHERE	i1.CONSTRAINT_TYPE = 'PRIMARY KEY'
		                ) PT ON PT.TABLE_NAME = PK.TABLE_NAME

                ORDER BY
                C.CONSTRAINT_NAME
                */
                  
                StringBuilder sb = new StringBuilder();

                sb.Append(" USE [" + _Name +"]");
                sb.Append(" SELECT C.CONSTRAINT_NAME AS [ConstraintName],");
		        sb.Append(" FK.TABLE_NAME AS FKTable,");
		        sb.Append(" CU.COLUMN_NAME AS FKColumn,");
		        sb.Append(" PK.TABLE_NAME AS PKTable,");
		        sb.Append(" PT.COLUMN_NAME AS PKColumn");

                sb.Append(" FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS C");
		        sb.Append(" INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS FK ON C.CONSTRAINT_NAME = FK.CONSTRAINT_NAME");
		        sb.Append(" INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS PK ON C.UNIQUE_CONSTRAINT_NAME = PK.CONSTRAINT_NAME");
		        sb.Append(" INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE CU ON C.CONSTRAINT_NAME = CU.CONSTRAINT_NAME");
		        sb.Append(" INNER JOIN (");
			    sb.Append(" SELECT i1.TABLE_NAME,"); 
				sb.Append(" i2.COLUMN_NAME");
			    sb.Append(" FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS i1");
				sb.Append(" INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE i2 ON i1.CONSTRAINT_NAME = i2.CONSTRAINT_NAME");
			    sb.Append(" WHERE i1.CONSTRAINT_TYPE = 'PRIMARY KEY'");
		        sb.Append(" ) PT ON PT.TABLE_NAME = PK.TABLE_NAME");
                sb.Append(" ORDER BY");
                sb.Append(" C.CONSTRAINT_NAME"); 

                return sb.ToString();
            }

            protected string GetDefaultValues()
            {
                /*
                USE [<db>] 
                 
                SELECT	sys.Objects.[Name]  AS [TableName],
		                syscolumns.name     AS [ColumnName],
		                syscomments.text    AS [DefaultValue],
                FROM	sys.objects 
		                INNER JOIN syscolumns ON sys.objects.[object_id] = syscolumns.[id]
		                INNER JOIN syscomments ON syscomments.id = syscolumns.cdefault
                WHERE	syscolumns.cdefault > 0
                AND	    is_ms_shipped = 0 
                ORDER	BY sys.objects.[name],syscolumns.colorder
                */
                
                StringBuilder sb = new StringBuilder();

                sb.Append(" USE [" + _Name +"]");
                sb.Append(" SELECT sys.Objects.[Name] AS [TableName],");
		        sb.Append(" syscolumns.[Name] AS [ColumnName],");
                sb.Append(" syscomments.text AS [DefaultValue]");
                sb.Append(" FROM sys.objects");
		        sb.Append(" INNER JOIN syscolumns ON sys.objects.[object_id] = syscolumns.[id]");
                sb.Append(" INNER JOIN syscomments ON syscomments.id = syscolumns.cdefault");
                sb.Append(" WHERE syscolumns.cdefault > 0");
                sb.Append(" AND	is_ms_shipped = 0");
                sb.Append(" ORDER BY sys.objects.[name],syscolumns.colorder");

                return sb.ToString();
            }

            /// <summary>
            /// gets rid of characters that wrap a sql default value
            /// Ex: ('Something') -> Something
            /// </summary>
            protected string RemoveWrappingCharacters(string input)
            {
                if (input.Length > 1 && (input[0] == '(' || input[0] == '\''))
                    input = input.Substring(1,input.Length-2);

                if (input.Length > 1 && (input[0] == '(' || input[0] == '\''))
                    input = input.Substring(1,input.Length-2);

                return input;
            }
    
        #endregion
    }
}
