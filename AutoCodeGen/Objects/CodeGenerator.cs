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
using System.Configuration;
using System.Data;
using System.Text;

using DAL.SqlMetadata;
using AutoCodeGen;
using log4net;

namespace AutoCodeGenLibrary
{
    public class CodeGenerator
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(CodeGenerator));

        /// <summary>
        /// This object is a collection of characters that could create problems if they are used in c# variable names. 
        /// This object is a 'hit list' of characters to remove.
        /// </summary>
        internal static string[] s_CSharpUndesireables = new string[] { "!", "$", "%", "^", "*", "(", ")", "-", "+", "=", "{", "}", "[", "]", ":", ";", "|", "'", "<", ">", ",", ".", "?", "/", " ", "~", "`", "\"", "\\" };
        internal static int[] s_PrimeList = new int[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97, 101, 103, 107, 109, 113, 127, 131, 137, 139, 149, 151, 157, 163, 167, 173, 179, 181, 191, 193, 197, 199, 211, 223, 227, 229, 233, 239, 241, 251, 257, 263, 269, 271 };

        protected readonly static string SQL_PARAMETER_TEMPLATE = "parameters.Add(new SqlParameter() {{ ParameterName = \"{0}\", SqlDbType = SqlDbType.{1}, Size = {2}, Value = {3} }});";

        internal static string s_SpNamePrefix = ConfigurationManager.AppSettings["SPNamePrefix"];
        internal static string s_SelectSingleSpSuffix = ConfigurationManager.AppSettings["SelectSingleSPSuffix"];
        internal static string s_SelectManySpSuffix = ConfigurationManager.AppSettings["SelectManySPSuffix"];
        internal static string s_SelectManyByXSpSuffix = ConfigurationManager.AppSettings["SelectManyByXSPSuffix"];
        internal static string s_SelectAllSpSuffix = ConfigurationManager.AppSettings["SelectAllSPSuffix"];
        internal static string s_SelectAllPagSpSuffix = ConfigurationManager.AppSettings["SelectAllPagSPSuffix"];
        internal static string s_InsertSingleSpSuffix = ConfigurationManager.AppSettings["InsertSingleSPSuffix"];
        internal static string s_UpdateSpSuffix = ConfigurationManager.AppSettings["UpdateSPSuffix"];
        internal static string s_UpdateInsertSpSuffix = ConfigurationManager.AppSettings["UpdateInsertSPSuffix"];
        internal static string s_DelAllSpSuffix = ConfigurationManager.AppSettings["DelAllSPSuffix"];
        internal static string s_DelManySpSuffix = ConfigurationManager.AppSettings["DelManySPSuffix"];
        internal static string s_DelSingleSpSuffix = ConfigurationManager.AppSettings["DelSingleSPSuffix"];
        internal static string s_CountAllSpSuffix = ConfigurationManager.AppSettings["CountAllSPSuffix"];
        internal static string s_CountSearchSpSuffix = ConfigurationManager.AppSettings["CountSearchSPSuffix"];

        internal static string s_DefaultDbUserName = ConfigurationManager.AppSettings["DefaultDbUserName"];

        // figure out tab size
        internal static int s_SQLTabSize = Convert.ToInt32(ConfigurationManager.AppSettings["SQLTabSize"]);
        internal static int s_VisualStudioTabSize = Convert.ToInt32(ConfigurationManager.AppSettings["VisualStudioTabSize"]);

        // SQL Procs
        public static OutputObject GenerateSelectSingleProc(SqlTable sqlTable, bool generate_stored_proc_perms)
        {
            if (sqlTable == null)
                return null;

            string procedure_name = GenerateSqlStoredProcName(sqlTable.Name, eStoredProcType.SelectSingle, null);
            int longest_column = GetLongestColumnLength(sqlTable) + sqlTable.Name.Length;
            bool flag_first_value;
            string name_buffer;

            OutputObject output = new OutputObject();
            output.Name = procedure_name + ".sql";
            output.Type = OutputObject.eObjectType.Sql;

            // sanity check - if there are no primary keys in the table, we cannot know 
            // what would a good choice would be for a qualifier. Abort and return error msg.
            if (sqlTable.PkList.Count == 0)
            {
                output.Body = "/* Cannot generate " + procedure_name + ", no primary keys set on " + sqlTable.Name + ". */" + Environment.NewLine + Environment.NewLine;
                return output;
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(GenerateSqlExistanceChecker(procedure_name));
            sb.AppendLine(GenerateHeader(procedure_name));

            sb.AppendLine("CREATE PROCEDURE " + procedure_name);
            sb.AppendLine(GenerateSqlStoredProcParameters(sqlTable, eIncludedFields.PKOnly));

            sb.AppendLine("AS");
            sb.AppendLine();
            //sb.AppendLine("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED");
            sb.AppendLine("SET NOCOUNT ON");
            sb.AppendLine();

            #region Selected Columns

            flag_first_value = true;

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if (flag_first_value)
                {
                    sb.Append("SELECT" + AddTabs(1));
                    flag_first_value = false;
                }
                else
                {
                    sb.Append("," + Environment.NewLine + AddTabs(2));
                }

                name_buffer = NameFormatter.ToTSQLName(sql_column.Table.Name) + "." + NameFormatter.ToTSQLName(sql_column.Name);
                sb.Append(PadSqlVariableName(name_buffer, longest_column) + "AS " + NameFormatter.ToTSQLName(sql_column.Name));
            }
            #endregion

            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("FROM" + AddTabs(1) + NameFormatter.ToTSQLName(sqlTable.Name));
            sb.AppendLine();

            #region Where Clause
            flag_first_value = true;
            sb.AppendLine("WHERE" + AddTabs(1) + PadSqlVariableName(NameFormatter.ToTSQLName(sqlTable.Name) + ".[Disabled]", longest_column) + "= 0");

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                string tsql_variablename = NameFormatter.ToTSQLVariableName(sql_column);

                if (sql_column.IsPk)
                {
                    name_buffer = NameFormatter.ToTSQLName(sql_column.Table.Name) + "." + NameFormatter.ToTSQLName(sql_column.Name);
                    sb.AppendLine("AND" + AddTabs(2) + PadSqlVariableName(name_buffer, longest_column) + "= " + tsql_variablename);
                }
            }
            #endregion

            sb.AppendLine("GO");
            sb.AppendLine();

            if (generate_stored_proc_perms)
            {
                sb.AppendLine(GenerateSqlStoredProcPerms(procedure_name));
                sb.AppendLine();
            }

            output.Body = sb.ToString();
            return output;
        }
        public static OutputObject GenerateSelectManyProc(SqlTable sqlTable, List<string> sort_fields, bool generate_stored_proc_perms)
        {
            if (sqlTable == null)
                return null;

            string procedure_name = GenerateSqlStoredProcName(sqlTable.Name, eStoredProcType.SelectMany, null);
            int longest_column = GetLongestColumnLength(sqlTable) + sqlTable.Name.Length;
            bool flag_first_value;
            string name_buffer;

            OutputObject output = new OutputObject();
            output.Name = procedure_name + ".sql";
            output.Type = OutputObject.eObjectType.Sql;

            // sanity check - if there are no primary keys in the table, we cannot know 
            // what would a good choice would be for a qualifier. Abort and return error msg.
            if (sqlTable.PkList.Count == 0)
            {
                output.Body = "/* Cannot generate " + procedure_name + ", no primary keys set on " + sqlTable.Name + ". */" + Environment.NewLine + Environment.NewLine;
                return output;
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(GenerateSqlExistanceChecker(procedure_name));
            sb.AppendLine(GenerateHeader(procedure_name));

            sb.AppendLine("CREATE PROCEDURE " + procedure_name);
            sb.AppendLine("(");
            sb.AppendLine(AddTabs(1) + "@IdList VARCHAR(MAX)");
            sb.AppendLine(")");
            sb.AppendLine("AS");
            sb.AppendLine();
            //sb.AppendLine("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED");
            sb.AppendLine("SET NOCOUNT ON");
            sb.AppendLine();

            sb.AppendLine(GenerateSqlListCode());

            #region Selected Columns

            flag_first_value = true;

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if (flag_first_value)
                {
                    sb.Append("SELECT" + AddTabs(1));
                    flag_first_value = false;
                }
                else
                {
                    sb.Append("," + Environment.NewLine + AddTabs(2));
                }

                name_buffer = NameFormatter.ToTSQLName(sql_column.Table.Name) + "." + NameFormatter.ToTSQLName(sql_column.Name);
                sb.Append(PadSqlVariableName(name_buffer, longest_column) + "AS " + NameFormatter.ToTSQLName(sql_column.Name));
            }
            #endregion

            sb.AppendLine();
            sb.AppendLine(Environment.NewLine + "FROM" + AddTabs(1) + NameFormatter.ToTSQLName(sqlTable.Name));
            sb.AppendLine();

            #region Where Clause

            sb.AppendLine("WHERE" + AddTabs(1) + PadSqlVariableName(NameFormatter.ToTSQLName(sqlTable.Name) + ".[Disabled]", longest_column) + "= 0");

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                string tsql_variablename = NameFormatter.ToTSQLVariableName(sql_column);

                if (sql_column.IsPk)
                {
                    sb.Append("AND" + AddTabs(2) + NameFormatter.ToTSQLName(sqlTable.Name) + "." + NameFormatter.ToTSQLName(sql_column.Name) + " in");
                    sb.AppendLine();
                    break;
                }
            }

            #endregion

            sb.AppendLine("(");
            sb.AppendLine(AddTabs(1) + "SELECT IdValue FROM @TableVar");
            sb.AppendLine(")");
            sb.AppendLine();
            sb.AppendLine(GenerateOrderByClause(sqlTable, sort_fields));
            sb.AppendLine("GO");
            sb.AppendLine();

            if (generate_stored_proc_perms)
            {
                sb.AppendLine(GenerateSqlStoredProcPerms(procedure_name));
                sb.AppendLine();
            }

            output.Body = sb.ToString();
            return output;
        }
        public static OutputObject GenerateSelectManyByXProc(SqlTable sqlTable, List<string> sort_fields, List<string> select_fields, bool generate_stored_proc_perms)
        {
            if (sqlTable == null)
                return null;

            string procedure_name = GenerateSqlStoredProcName(sqlTable.Name, eStoredProcType.SelectManyByX, select_fields);
            int longest_column = GetLongestColumnLength(sqlTable) + sqlTable.Name.Length;
            bool flag_first_value;
            string name_buffer;

            OutputObject output = new OutputObject();
            output.Name = procedure_name + ".sql";
            output.Type = OutputObject.eObjectType.Sql;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(GenerateSqlExistanceChecker(procedure_name));
            sb.AppendLine(GenerateHeader(procedure_name));

            sb.AppendLine("CREATE PROCEDURE " + procedure_name);
            sb.AppendLine("(");
            sb.AppendLine(GenerateSelectClauseArguments(sqlTable, select_fields));
            sb.AppendLine(")");
            sb.AppendLine("AS");
            sb.AppendLine();
            //sb.AppendLine("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED");
            sb.AppendLine("SET NOCOUNT ON");
            sb.AppendLine();

            #region Selected Columns

            flag_first_value = true;

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if (flag_first_value)
                {
                    sb.Append("SELECT" + AddTabs(1));
                    flag_first_value = false;
                }
                else
                {
                    sb.Append("," + Environment.NewLine + AddTabs(2));
                }

                name_buffer = NameFormatter.ToTSQLName(sql_column.Table.Name) + "." + NameFormatter.ToTSQLName(sql_column.Name);
                sb.Append(PadSqlVariableName(name_buffer, longest_column) + "AS " + NameFormatter.ToTSQLName(sql_column.Name));
            }
            #endregion

            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("FROM" + AddTabs(1) + NameFormatter.ToTSQLName(sqlTable.Name));
            sb.AppendLine();

            sb.AppendLine("WHERE" + AddTabs(1) + PadSqlVariableName(NameFormatter.ToTSQLName(sqlTable.Name) + ".[Disabled]", longest_column) + "= 0");
            sb.AppendLine(GenerateSelectClause(sqlTable, select_fields));
            sb.AppendLine();
            sb.AppendLine(GenerateOrderByClause(sqlTable, sort_fields));
            sb.AppendLine("GO");
            sb.AppendLine();

            if (generate_stored_proc_perms)
            {
                sb.AppendLine(GenerateSqlStoredProcPerms(procedure_name));
                sb.AppendLine();
            }

            output.Body = sb.ToString();
            return output;
        }
        public static OutputObject GenerateSelectAllProc(SqlTable sqlTable, List<string> sort_fields, bool generate_stored_proc_perms)
        {
            if (sqlTable == null)
                return null;

            string procedure_name = GenerateSqlStoredProcName(sqlTable.Name, eStoredProcType.SelectAll, null);
            int longest_column = GetLongestColumnLength(sqlTable) + sqlTable.Name.Length;
            bool flag_first_value;
            string name_buffer;

            OutputObject output = new OutputObject();
            output.Name = procedure_name + ".sql";
            output.Type = OutputObject.eObjectType.Sql;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(GenerateSqlExistanceChecker(procedure_name));
            sb.AppendLine(GenerateHeader(procedure_name));

            sb.AppendLine("CREATE PROCEDURE " + procedure_name);
            sb.AppendLine("AS");
            sb.AppendLine();
            //sb.AppendLine("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED");
            sb.AppendLine("SET NOCOUNT ON");
            sb.AppendLine();

            #region Selected Columns

            flag_first_value = true;

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if (flag_first_value)
                {
                    sb.Append("SELECT" + AddTabs(1));
                    flag_first_value = false;
                }
                else
                {
                    sb.Append("," + Environment.NewLine + AddTabs(2));
                }

                name_buffer = NameFormatter.ToTSQLName(sql_column.Table.Name) + "." + NameFormatter.ToTSQLName(sql_column.Name);
                sb.Append(PadSqlVariableName(name_buffer, longest_column) + "AS " + NameFormatter.ToTSQLName(sql_column.Name));
            }
            #endregion

            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("FROM" + AddTabs(1) + NameFormatter.ToTSQLName(sqlTable.Name));
            sb.AppendLine();
            sb.AppendLine("WHERE" + AddTabs(1) + PadSqlVariableName(NameFormatter.ToTSQLName(sqlTable.Name) + ".[Disabled]", longest_column) + "= 0");
            sb.AppendLine();
            sb.AppendLine(GenerateOrderByClause(sqlTable, sort_fields));
            sb.AppendLine("GO");
            sb.AppendLine();

            if (generate_stored_proc_perms)
            {
                sb.AppendLine(GenerateSqlStoredProcPerms(procedure_name));
                sb.AppendLine();
            }

            output.Body = sb.ToString();
            return output;
        }
        public static OutputObject GenerateSelectAllPaginatedProc(SqlTable sqlTable, List<string> sort_fields, List<string> search_fields, bool generate_stored_proc_perms)
        {
            if (sqlTable == null)
                return null;

            string procedure_name = GenerateSqlStoredProcName(sqlTable.Name, eStoredProcType.SelectAllPag, null);
            int longest_column = GetLongestColumnLength(sqlTable) + sqlTable.Name.Length;
            string name_buffer;

            OutputObject output = new OutputObject();
            output.Name = procedure_name + ".sql";
            output.Type = OutputObject.eObjectType.Sql;

            // sanity check - if there are no primary keys in the table, we cannot know 
            // what would a good choice would be for a qualifier. Abort and return error msg.
            if (sqlTable.PkList.Count == 0)
            {
                output.Body = "/* Cannot generate " + procedure_name + ", no primary keys set on " + sqlTable.Name + ". */" + Environment.NewLine + Environment.NewLine;
                return output;
            }

            #region Find the first pk
            string table_pk = string.Empty;

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                string tsql_variablename = NameFormatter.ToTSQLName(sql_column.Name);

                if (sql_column.IsPk)
                {
                    table_pk = sql_column.Name;
                    break;
                }
            }
            #endregion

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(GenerateSqlExistanceChecker(procedure_name));
            sb.AppendLine(GenerateHeader(procedure_name));

            sb.AppendLine("CREATE PROCEDURE " + procedure_name);
            sb.AppendLine("(");
            sb.AppendLine(AddTabs(1) + "@StartNumber INT,");
            sb.AppendLine(AddTabs(1) + "@PageSize INT,");
            sb.AppendLine(AddTabs(1) + "@SearchString VARCHAR(50)");
            sb.AppendLine(")");

            sb.AppendLine("AS");
            sb.AppendLine();

            //sb.AppendLine("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED");
            sb.AppendLine("SET NOCOUNT ON");
            sb.AppendLine();

            sb.AppendLine("SELECT" + AddTabs(1) + "*");
            sb.AppendLine("FROM");
            sb.AppendLine("(");
            sb.AppendLine(AddTabs(1) + "SELECT ROW_NUMBER()");
            sb.Append(AddTabs(1) + "OVER (" + GenerateOrderByClause(sqlTable, sort_fields) + ") AS Row");

            #region Selected Columns

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                name_buffer = NameFormatter.ToTSQLName(sql_column.Table.Name) + "." + NameFormatter.ToTSQLName(sql_column.Name);

                sb.Append("," + Environment.NewLine + AddTabs(3));
                sb.Append(PadSqlVariableName(name_buffer, longest_column) + "AS " + NameFormatter.ToTSQLName(sql_column.Name));
            }
            #endregion

            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine(AddTabs(1) + "FROM" + AddTabs(1) + NameFormatter.ToTSQLName(sqlTable.Name));
            sb.AppendLine();
            sb.AppendLine(GenerateSearchClause(sqlTable, search_fields, 1, 1));
            sb.AppendLine(AddTabs(1) + "AND" + AddTabs(2) + NameFormatter.ToTSQLName(sqlTable.Name) + ".[Disabled] = 0");
            sb.AppendLine(") AS Page");
            sb.AppendLine("WHERE Page.Row >= @StartNumber");
            sb.AppendLine("AND Page.Row < @PageSize + @StartNumber");
            sb.AppendLine("GO");
            sb.AppendLine();

            if (generate_stored_proc_perms)
            {
                sb.AppendLine(GenerateSqlStoredProcPerms(procedure_name));
                sb.AppendLine();
            }

            output.Body = sb.ToString();
            return output;
        }
        public static OutputObject GenerateSetProc(SqlTable sqlTable, bool generate_stored_proc_perms)
        {
            if (sqlTable == null)
                return null;

            string procedure_name = GenerateSqlStoredProcName(sqlTable.Name, eStoredProcType.UpdateInsert, null);
            int longest_column = GetLongestColumnLength(sqlTable);
            bool flag_first_value = true;

            OutputObject output = new OutputObject();
            output.Name = procedure_name + ".sql";
            output.Type = OutputObject.eObjectType.Sql;

            // sanity check - if there are no primary keys in the table, we cannot know 
            // what would a good choice would be for a qualifier. Abort and return error msg.
            if (sqlTable.PkList.Count == 0)
            {
                output.Body = "/* Cannot generate " + procedure_name + ", no primary keys set on " + sqlTable.Name + ". */" + Environment.NewLine + Environment.NewLine;
                return output;
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(GenerateSqlExistanceChecker(procedure_name));
            sb.AppendLine(GenerateHeader(procedure_name));

            sb.AppendLine("CREATE PROCEDURE " + procedure_name);
            sb.AppendLine(GenerateSqlStoredProcParameters(sqlTable, eIncludedFields.All));

            sb.AppendLine("AS");
            sb.AppendLine();
            sb.AppendLine("SET NOCOUNT ON");
            sb.AppendLine();

            #region Existance Check
            sb.Append("IF EXISTS (SELECT 1 FROM [" + sqlTable.Name + "] ");

            // Build where clause
            flag_first_value = true;

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if (sql_column.IsPk)
                {
                    if (flag_first_value)
                    {
                        sb.Append("WHERE ");
                        flag_first_value = false;
                    }
                    else
                    {
                        sb.Append(" AND ");
                    }

                    sb.Append(sql_column.Name + " = " + NameFormatter.ToTSQLVariableName(sql_column));
                }
            }

            sb.AppendLine(")");
            #endregion

            sb.AppendLine("BEGIN");

            #region Update Clause
            sb.Append(AddTabs(1) + "-- Already Exists, Update" + Environment.NewLine);
            sb.Append(AddTabs(1) + "UPDATE\t[" + sqlTable.Name + "]" + Environment.NewLine);

            // list selected columns
            flag_first_value = true;

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if (!sql_column.IsPk && !sql_column.IsIdentity)
                {
                    if (flag_first_value)
                    {
                        sb.Append(AddTabs(1) + "SET" + AddTabs(2));
                        flag_first_value = false;
                    }
                    else
                    {
                        sb.Append("," + Environment.NewLine + AddTabs(3));
                    }

                    sb.Append(PadSqlVariableName(NameFormatter.ToTSQLName(sql_column.Name), longest_column) + "= " + NameFormatter.ToTSQLVariableName(sql_column));
                }
            }

            sb.Append(Environment.NewLine);

            // Build where clause
            flag_first_value = true;

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if (sql_column.IsPk)
                {
                    if (flag_first_value)
                    {
                        sb.Append(AddTabs(1) + "WHERE" + AddTabs(1));
                        flag_first_value = false;
                    }
                    else
                    {
                        sb.Append(Environment.NewLine + AddTabs(1) + "AND" + AddTabs(1));
                    }

                    sb.Append(PadSqlVariableName(NameFormatter.ToTSQLName(sql_column.Name), longest_column) + "= " + NameFormatter.ToTSQLVariableName(sql_column));
                }
            }

            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine(AddTabs(1) + "RETURN @Id"); // HACK - this will work with my PK designs....
            #endregion

            sb.AppendLine("END");
            sb.AppendLine("ELSE");
            sb.AppendLine("BEGIN");

            #region Insert Clause
            sb.AppendLine(AddTabs(1) + "-- New Entry, Insert");
            sb.AppendLine(AddTabs(1) + "INSERT [" + sqlTable.Name + "]");
            sb.AppendLine(AddTabs(1) + "(");

            // list selected columns
            flag_first_value = true;

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if (!sql_column.IsIdentity)
                {
                    if (flag_first_value)
                    {
                        sb.Append(AddTabs(2));
                        flag_first_value = false;
                    }
                    else
                    {
                        sb.Append("," + Environment.NewLine + AddTabs(2));
                    }

                    sb.Append(NameFormatter.ToTSQLName(sql_column.Name));
                }
            }

            sb.Append(Environment.NewLine + AddTabs(1) + ")");
            sb.Append(Environment.NewLine + AddTabs(1) + "VALUES");
            sb.Append(Environment.NewLine + AddTabs(1) + "(");

            // Build where clause
            flag_first_value = true;

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if (!sql_column.IsIdentity)
                {
                    if (flag_first_value)
                    {
                        sb.Append(Environment.NewLine + AddTabs(2));
                        flag_first_value = false;
                    }
                    else
                    {
                        sb.Append("," + Environment.NewLine + AddTabs(2));
                    }

                    sb.Append(NameFormatter.ToTSQLVariableName(sql_column));
                }
            }

            sb.Append(Environment.NewLine + AddTabs(1) + ")");
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine(AddTabs(1) + "RETURN SCOPE_IDENTITY()");
            #endregion

            sb.AppendLine("END");
            sb.AppendLine("GO");
            sb.AppendLine();

            if (generate_stored_proc_perms)
            {
                sb.AppendLine(GenerateSqlStoredProcPerms(procedure_name));
                sb.AppendLine();
            }

            output.Body = sb.ToString();
            return output;
        }
        public static OutputObject GenerateDeleteSingleProc(SqlTable sqlTable, bool generate_stored_proc_perms)
        {
            if (sqlTable == null)
                return null;

            string procedure_name = GenerateSqlStoredProcName(sqlTable.Name, eStoredProcType.DelSingle, null);
            bool flag_first_value = true;

            OutputObject output = new OutputObject();
            output.Name = procedure_name + ".sql";
            output.Type = OutputObject.eObjectType.Sql;

            // sanity check - if there are no primary keys in the table, we cannot know 
            // what would a good choice would be for a qualifier. Abort and return error msg.
            if (sqlTable.PkList.Count == 0)
            {
                output.Body = "/* Cannot generate " + procedure_name + ", no primary keys set on " + sqlTable.Name + ". */" + Environment.NewLine + Environment.NewLine;
                return output;
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(GenerateSqlExistanceChecker(procedure_name));
            sb.AppendLine(GenerateHeader(procedure_name));

            sb.AppendLine("CREATE PROCEDURE " + procedure_name);
            sb.AppendLine(GenerateSqlStoredProcParameters(sqlTable, eIncludedFields.PKOnly));

            sb.AppendLine("AS");
            sb.AppendLine();
            sb.AppendLine("SET NOCOUNT ON");
            sb.AppendLine();
            sb.AppendLine("UPDATE" + AddTabs(1) + NameFormatter.ToTSQLName(sqlTable.Name));
            sb.AppendLine("SET" + AddTabs(2) + "[Disabled] = 1");

            #region Where Clause
            flag_first_value = true;

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                // output:
                // WHERE    [UserName] = @UserName
                // AND      [UserAge] = @UserAge

                if (sql_column.IsPk)
                {
                    if (flag_first_value)
                    {
                        sb.Append("WHERE" + AddTabs(1));
                        flag_first_value = false;
                    }
                    else
                    {
                        sb.Append(Environment.NewLine + "AND" + AddTabs(2));
                    }

                    // TODO: Add padding
                    sb.Append(NameFormatter.ToTSQLName(sql_column.Name) + " = " + NameFormatter.ToTSQLVariableName(sql_column));
                }
            }
            #endregion

            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("RETURN @@ROWCOUNT");
            sb.AppendLine("GO");
            sb.AppendLine();

            if (generate_stored_proc_perms)
            {
                sb.AppendLine(GenerateSqlStoredProcPerms(procedure_name));
                sb.AppendLine();
            }

            output.Body = sb.ToString();
            return output;
        }
        public static OutputObject GenerateDeleteManyProc(SqlTable sqlTable, bool generate_stored_proc_perms)
        {
            if (sqlTable == null)
                return null;

            string procedure_name = GenerateSqlStoredProcName(sqlTable.Name, eStoredProcType.DelMany, null);

            OutputObject output = new OutputObject();
            output.Name = procedure_name + ".sql";
            output.Type = OutputObject.eObjectType.Sql;

            // sanity check - if there are no primary keys in the table, we cannot know 
            // what would a good choice would be for a qualifier. Abort and return error msg.
            if (sqlTable.PkList.Count == 0)
            {
                output.Body = "/* Cannot generate " + procedure_name + ", no primary keys set on " + sqlTable.Name + ". */" + Environment.NewLine + Environment.NewLine;
                return output;
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(GenerateSqlExistanceChecker(procedure_name));
            sb.AppendLine(GenerateHeader(procedure_name));

            sb.AppendLine("CREATE PROCEDURE " + procedure_name);
            sb.AppendLine("(");
            sb.AppendLine(AddTabs(1) + "@IdList VARCHAR(MAX)");
            sb.AppendLine(")");
            sb.AppendLine("AS");
            sb.AppendLine();
            sb.AppendLine("SET NOCOUNT ON");
            sb.AppendLine();

            sb.AppendLine(GenerateSqlListCode());

            #region UPDATE statement
            sb.AppendLine("UPDATE" + AddTabs(1) + NameFormatter.ToTSQLName(sqlTable.Name));
            sb.AppendLine("SET" + AddTabs(2) + "[Disabled] = 1");

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                string tsql_variablename = NameFormatter.ToTSQLVariableName(sql_column);

                if (sql_column.IsPk)
                {
                    sb.Append("WHERE" + AddTabs(1) + NameFormatter.ToTSQLName(sql_column.Name) + " in");
                    sb.AppendLine();
                    break;
                }
            }

            sb.AppendLine("(");
            sb.AppendLine(AddTabs(1) + "SELECT IdValue FROM @TableVar");
            sb.AppendLine(")");
            #endregion

            sb.AppendLine();
            sb.AppendLine("RETURN @@ROWCOUNT");
            sb.AppendLine("GO");
            sb.AppendLine();

            if (generate_stored_proc_perms)
            {
                sb.AppendLine(GenerateSqlStoredProcPerms(procedure_name));
                sb.AppendLine();
            }

            output.Body = sb.ToString();
            return output;
        }
        public static OutputObject GenerateDeleteAllProc(SqlTable sqlTable, bool generate_stored_proc_perms)
        {
            if (sqlTable == null)
                return null;

            string procedure_name = GenerateSqlStoredProcName(sqlTable.Name, eStoredProcType.DelAll, null);

            OutputObject output = new OutputObject();
            output.Name = procedure_name + ".sql";
            output.Type = OutputObject.eObjectType.Sql;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(GenerateSqlExistanceChecker(procedure_name));
            sb.AppendLine(GenerateHeader(procedure_name));

            sb.AppendLine("CREATE PROCEDURE " + procedure_name);
            sb.AppendLine("AS");
            sb.AppendLine();
            sb.AppendLine("SET NOCOUNT ON");
            sb.AppendLine();
            sb.AppendLine("UPDATE\t[" + sqlTable.Name + "]");
            sb.AppendLine("SET\t\t[Disabled] = 1");
            sb.AppendLine();
            sb.AppendLine("RETURN @@ROWCOUNT");
            sb.AppendLine("GO");
            sb.AppendLine();

            if (generate_stored_proc_perms)
            {
                sb.AppendLine(GenerateSqlStoredProcPerms(procedure_name));
                sb.AppendLine();
            }

            output.Body = sb.ToString();
            return output;
        }
        public static OutputObject GenerateCountAllProc(SqlTable sqlTable, bool generate_stored_proc_perms)
        {
            if (sqlTable == null)
                return null;

            string procedure_name = GenerateSqlStoredProcName(sqlTable.Name, eStoredProcType.CountAll, null);

            OutputObject output = new OutputObject();
            output.Name = procedure_name + ".sql";
            output.Type = OutputObject.eObjectType.Sql;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(GenerateSqlExistanceChecker(procedure_name));
            sb.AppendLine(GenerateHeader(procedure_name));

            sb.AppendLine("CREATE PROCEDURE " + procedure_name);
            sb.AppendLine("AS");
            sb.AppendLine();
            //sb.AppendLine("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED");
            sb.AppendLine("SET NOCOUNT ON");
            sb.AppendLine();
            sb.AppendLine("SELECT\tCOUNT(*)");
            sb.AppendLine("FROM\t[" + sqlTable.Name + "]");
            sb.AppendLine("WHERE\t[Disabled] = 0");
            sb.AppendLine("GO");
            sb.AppendLine();

            if (generate_stored_proc_perms)
            {
                sb.AppendLine(GenerateSqlStoredProcPerms(procedure_name));
                sb.AppendLine();
            }

            output.Body = sb.ToString();
            return output;
        }
        public static OutputObject GenerateCountSearchProc(SqlTable sqlTable, List<string> search_fields, bool generate_stored_proc_perms)
        {
            if (sqlTable == null)
                return null;

            string procedure_name = GenerateSqlStoredProcName(sqlTable.Name, eStoredProcType.CountSearch, null);

            OutputObject output = new OutputObject();
            output.Name = procedure_name + ".sql";
            output.Type = OutputObject.eObjectType.Sql;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(GenerateSqlExistanceChecker(procedure_name));
            sb.AppendLine(GenerateHeader(procedure_name));

            sb.AppendLine("CREATE PROCEDURE " + procedure_name);
            sb.AppendLine("(");
            sb.AppendLine(AddTabs(1) + "@SearchString VARCHAR(50)");
            sb.AppendLine(")");
            sb.AppendLine("AS");
            sb.AppendLine();
            //sb.AppendLine("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED");
            sb.AppendLine("SET NOCOUNT ON");
            sb.AppendLine();
            sb.AppendLine("SELECT" + AddTabs(1) + "COUNT(*)");
            sb.AppendLine();
            sb.AppendLine("FROM" + AddTabs(1) + "[" + sqlTable.Name + "]");
            sb.AppendLine();
            sb.AppendLine(GenerateSearchClause(sqlTable, search_fields, 0, 1));
            sb.AppendLine("AND" + AddTabs(2) + NameFormatter.ToTSQLName(sqlTable.Name) + ".[Disabled] = 0");
            sb.AppendLine("GO");
            sb.AppendLine();

            if (generate_stored_proc_perms)
            {
                sb.AppendLine(GenerateSqlStoredProcPerms(procedure_name));
                sb.AppendLine();
            }

            output.Body = sb.ToString();
            return output;
        }

        public static string GenerateInlineSelectSingleProc(SqlTable sqlTable)
        {
            if (sqlTable == null)
                return null;

            bool flag_first_value;

            // sanity check - if there are no primary keys in the table, we cannot know 
            // what would a good choice would be for a qualifier. Abort and return error msg.
            if (sqlTable.PkList.Count == 0)
                return "/* Cannot generate InlineSelectSingleProc, no primary keys set on " + sqlTable.Name + ". */";

            StringBuilder sb = new StringBuilder();

            #region Selected Columns

            flag_first_value = true;

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if (flag_first_value)
                {
                    sb.Append("SELECT ");
                    flag_first_value = false;
                }
                else
                {
                    sb.Append(",");
                }

                sb.Append(NameFormatter.ToTSQLName(sql_column.Name));
            }
            #endregion

            sb.Append(" FROM " + NameFormatter.ToTSQLName(sqlTable.Name));

            #region Where Clause

            flag_first_value = true;
            sb.Append(" WHERE" + NameFormatter.ToTSQLName(sqlTable.Name) + ".[Disabled] = 0");

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                string tsql_variablename = NameFormatter.ToTSQLVariableName(sql_column);

                if (sql_column.IsPk)
                    sb.Append(" AND " + NameFormatter.ToTSQLName(sql_column.Name) + " = " + tsql_variablename);
            }
            #endregion

            return sb.ToString();
        }
        public static string GenerateInlineSelectManyProc(SqlTable sqlTable, List<string> sort_fields)
        {
            if (sqlTable == null)
                return null;

            bool flag_first_value;

            // sanity check - if there are no primary keys in the table, we cannot know 
            // what would a good choice would be for a qualifier. Abort and return error msg.
            if (sqlTable.PkList.Count == 0)
                return "/* Cannot generate InlineSelectManyProc, no primary keys set on " + sqlTable.Name + ". */";

            StringBuilder sb = new StringBuilder();

            #region Selected Columns

            flag_first_value = true;

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if (flag_first_value)
                {
                    sb.Append("SELECT ");
                    flag_first_value = false;
                }
                else
                {
                    sb.Append(",");
                }

                sb.Append(NameFormatter.ToTSQLName(sql_column.Name));
            }
            #endregion

            sb.Append(" FROM " + NameFormatter.ToTSQLName(sqlTable.Name));

            #region Where Clause

            sb.Append(" WHERE " + NameFormatter.ToTSQLName(sqlTable.Name) + ".[Disabled] = 0");

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                string tsql_variablename = NameFormatter.ToTSQLVariableName(sql_column);

                if (sql_column.IsPk)
                {
                    sb.Append(" AND " + NameFormatter.ToTSQLName(sqlTable.Name) + "." + NameFormatter.ToTSQLName(sql_column.Name) + " IN");
                    break;
                }
            }

            #endregion

            sb.Append(" (SELECT IdValue FROM @TableVar) ");
            sb.Append(GenerateOrderByClause(sqlTable, sort_fields));

            return sb.ToString();
        }
        public static string GenerateInlineSelectManyByXProc(SqlTable sqlTable, List<string> sort_fields, List<string> select_fields)
        {
            if (sqlTable == null)
                return null;

            bool flag_first_value = true;

            StringBuilder sb = new StringBuilder();

            #region Selected Columns

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if (flag_first_value)
                {
                    sb.Append("SELECT ");
                    flag_first_value = false;
                }
                else
                {
                    sb.Append(",");
                }

                sb.Append(NameFormatter.ToTSQLName(sql_column.Name));
            }
            #endregion

            sb.Append(" FROM " + NameFormatter.ToTSQLName(sqlTable.Name));
            sb.Append(" WHERE " + NameFormatter.ToTSQLName(sqlTable.Name) + ".[Disabled] = 0");
            sb.Append(GenerateSelectClause(sqlTable, select_fields));
            sb.Append(GenerateOrderByClause(sqlTable, sort_fields));

            return sb.ToString();
        }
        public static string GenerateInlineSelectAllProc(SqlTable sqlTable, List<string> sort_fields)
        {
            if (sqlTable == null)
                return null;

            bool flag_first_value;

            StringBuilder sb = new StringBuilder();

            #region Selected Columns

            flag_first_value = true;

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if (flag_first_value)
                {
                    sb.Append("SELECT ");
                    flag_first_value = false;
                }
                else
                {
                    sb.Append(",");
                }

                sb.Append(NameFormatter.ToTSQLName(sql_column.Table.Name) + "." + NameFormatter.ToTSQLName(sql_column.Name));
            }
            #endregion

            sb.Append(" FROM " + NameFormatter.ToTSQLName(sqlTable.Name));
            sb.Append(" WHERE " + NameFormatter.ToTSQLName(sqlTable.Name) + ".[Disabled] = 0");
            sb.Append(GenerateOrderByClause(sqlTable, sort_fields));

            return sb.ToString();
        }
        public static string GenerateInlineSelectAllPaginatedProc(SqlTable sqlTable, List<string> sort_fields, List<string> search_fields)
        {
            if (sqlTable == null)
                return null;

            // sanity check - if there are no primary keys in the table, we cannot know 
            // what would a good choice would be for a qualifier. Abort and return error msg.
            if (sqlTable.PkList.Count == 0)
                return "/* Cannot generate InlineSelectAllPaginatedProc, no primary keys set on " + sqlTable.Name + ". */";

            #region Find the first pk
            string table_pk = string.Empty;

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                string tsql_variablename = NameFormatter.ToTSQLName(sql_column.Name);

                if (sql_column.IsPk)
                {
                    table_pk = sql_column.Name;
                    break;
                }
            }
            #endregion

            StringBuilder sb = new StringBuilder();

            sb.Append("SELECT * ");
            sb.Append("FROM ");
            sb.Append("(");
            sb.Append("SELECT ROW_NUMBER() ");
            sb.Append("OVER (" + GenerateOrderByClause(sqlTable, sort_fields) + ") AS Row");

            #region Selected Columns

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                sb.Append(",");
                sb.Append(NameFormatter.ToTSQLName(sql_column.Table.Name) + "." + NameFormatter.ToTSQLName(sql_column.Name));
            }
            #endregion

            sb.Append(" FROM " + NameFormatter.ToTSQLName(sqlTable.Name));

            sb.Append(GenerateSearchClause(sqlTable, search_fields, 1, 1));
            sb.Append(" AND " + NameFormatter.ToTSQLName(sqlTable.Name) + ".[Disabled] = 0");
            sb.Append(") AS Page ");
            sb.Append("WHERE Page.Row >= @StartNumber ");
            sb.Append("AND Page.Row < @PageSize + @StartNumber ");

            return sb.ToString();
        }
        public static string GenerateInlineSetProc(SqlTable sqlTable)
        {
            if (sqlTable == null)
                return null;

            bool flag_first_value = true;

            // sanity check - if there are no primary keys in the table, we cannot know 
            // what would a good choice would be for a qualifier. Abort and return error msg.
            if (sqlTable.PkList.Count == 0)
                return "/* Cannot generate InlineSetProc, no primary keys set on " + sqlTable.Name + ". */";

            StringBuilder sb = new StringBuilder();

            #region Existance Check
            sb.Append("IF EXISTS (SELECT 1 FROM [" + sqlTable.Name + "] ");

            // Build where clause
            flag_first_value = true;

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if (sql_column.IsPk)
                {
                    if (flag_first_value)
                    {
                        sb.Append(" WHERE ");
                        flag_first_value = false;
                    }
                    else
                    {
                        sb.Append(" AND ");
                    }

                    sb.Append(sql_column.Name + " = " + NameFormatter.ToTSQLVariableName(sql_column));
                }
            }

            sb.Append(")");
            #endregion

            sb.Append(" BEGIN ");

            #region Update Clause

            sb.Append(" UPDATE [" + sqlTable.Name + "]");

            // list selected columns
            flag_first_value = true;

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if (!sql_column.IsPk && !sql_column.IsIdentity)
                {
                    if (flag_first_value)
                    {
                        sb.Append(" SET ");
                        flag_first_value = false;
                    }
                    else
                    {
                        sb.Append(",");
                    }

                    sb.Append(NameFormatter.ToTSQLName(sql_column.Name) + " = " + NameFormatter.ToTSQLVariableName(sql_column));
                }
            }

            // Build where clause
            flag_first_value = true;

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if (sql_column.IsPk)
                {
                    if (flag_first_value)
                    {
                        sb.Append(" WHERE ");
                        flag_first_value = false;
                    }
                    else
                    {
                        sb.Append(" AND ");
                    }

                    sb.Append(NameFormatter.ToTSQLName(sql_column.Name) + " = " + NameFormatter.ToTSQLVariableName(sql_column));
                }
            }

            sb.Append(" RETURN SCOPE_IDENTITY() ");
            #endregion

            sb.Append(" END ");
            sb.Append(" ELSE ");
            sb.Append(" BEGIN ");

            #region Insert Clause
            sb.Append(" INSERT [" + sqlTable.Name + "]");
            sb.Append("(");

            // list selected columns
            flag_first_value = true;

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if (!sql_column.IsIdentity)
                {
                    if (flag_first_value)
                    {
                        flag_first_value = false;
                    }
                    else
                    {
                        sb.Append(",");
                    }

                    sb.Append(NameFormatter.ToTSQLName(sql_column.Name));
                }
            }

            sb.Append(")");
            sb.Append(" VALUES ");
            sb.Append("(");

            // Build where clause
            flag_first_value = true;

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if (!sql_column.IsIdentity)
                {
                    if (flag_first_value)
                    {
                        flag_first_value = false;
                    }
                    else
                    {
                        sb.Append(",");
                    }

                    sb.Append(NameFormatter.ToTSQLVariableName(sql_column));
                }
            }

            sb.Append(")");


            sb.Append("RETURN SCOPE_IDENTITY()");
            #endregion

            sb.Append(" END ");

            return sb.ToString();
        }
        public static string GenerateInlineDeleteSingleProc(SqlTable sqlTable)
        {
            if (sqlTable == null)
                return null;

            bool flag_first_value = true;

            // sanity check - if there are no primary keys in the table, we cannot know 
            // what would a good choice would be for a qualifier. Abort and return error msg.
            if (sqlTable.PkList.Count == 0)
                return "/* Cannot generate InlineDeleteSingleProc, no primary keys set on " + sqlTable.Name + ". */";

            StringBuilder sb = new StringBuilder();

            sb.Append(" UPDATE " + NameFormatter.ToTSQLName(sqlTable.Name));
            sb.Append(" SET [Disabled] = 1 ");

            #region Where Clause
            flag_first_value = true;

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                // output:
                // WHERE    [UserName] = @UserName
                // AND      [UserAge] = @UserAge

                if (sql_column.IsPk)
                {
                    if (flag_first_value)
                    {
                        sb.Append(" WHERE ");
                        flag_first_value = false;
                    }
                    else
                    {
                        sb.Append(" AND ");
                    }

                    // TODO: Add padding
                    sb.Append(NameFormatter.ToTSQLName(sql_column.Name) + " = " + NameFormatter.ToTSQLVariableName(sql_column));
                }
            }
            #endregion

            sb.Append(" RETURN @@ROWCOUNT ");

            return sb.ToString();
        }
        public static string GenerateInlineDeleteManyProc(SqlTable sqlTable)
        {
            if (sqlTable == null)
                return null;

            // sanity check - if there are no primary keys in the table, we cannot know 
            // what would a good choice would be for a qualifier. Abort and return error msg.
            if (sqlTable.PkList.Count == 0)
                return "/* Cannot generate InlineDeleteManyProc, no primary keys set on " + sqlTable.Name + ". */";

            StringBuilder sb = new StringBuilder();

            sb.Append(GenerateSqlListCode());

            #region UPDATE statement
            sb.Append(" UPDATE " + NameFormatter.ToTSQLName(sqlTable.Name));
            sb.Append(" SET [Disabled] = 1");

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                string tsql_variablename = NameFormatter.ToTSQLVariableName(sql_column);

                if (sql_column.IsPk)
                {
                    sb.Append(" WHERE " + NameFormatter.ToTSQLName(sql_column.Name) + " in ");
                    break;
                }
            }

            sb.Append("(");
            sb.Append("SELECT IdValue FROM @TableVar");
            sb.Append(")");
            #endregion

            sb.Append(" RETURN @@ROWCOUNT ");

            return sb.ToString();
        }
        public static string GenerateInlineDeleteAllProc(SqlTable sqlTable)
        {
            if (sqlTable == null)
                return null;

            StringBuilder sb = new StringBuilder();

            sb.Append(string.Format("UPDATE [{0}].[dbo].[{1}] SET [Disabled] = 1 RETURN @@ROWCOUNT", sqlTable.Name, sqlTable.Name));

            return sb.ToString();
        }
        public static string GenerateInlineCountAllProc(SqlTable sqlTable)
        {
            if (sqlTable == null)
                return null;

            StringBuilder sb = new StringBuilder();

            sb.Append(string.Format("SELECT COUNT(*) FROM [{0}].[dbo].[{1}] WHERE [Disabled] = 0", sqlTable.Database.Name, sqlTable.Name));

            return sb.ToString();
        }
        public static string GenerateInlineCountSearchProc(SqlTable sqlTable, List<string> search_fields)
        {
            if (sqlTable == null)
                return null;

            StringBuilder sb = new StringBuilder();

            sb.Append(string.Format("SELECT COUNT(*) FROM [{0}].[dbo].[{1}] ", sqlTable.Database.Name, sqlTable.Name));
            sb.Append(GenerateSearchClause(sqlTable, search_fields, 0, 1));
            sb.Append("AND " + NameFormatter.ToTSQLName(sqlTable.Name) + ".[Disabled] = 0 ");

            return sb.ToString();
        }

        // C# Code Generation
        public static OutputObject GenerateCSharpOrmClass(SqlTable sqlTable, List<string> namespaceIncludes, bool includeClassDecoration)
        {
            if (sqlTable == null)
                return null;

            //todo: wrire these up somewhere
            bool include_region_blocks = false;
            bool include_static_field_names = false;
            bool include_to_string_overload = false;
            bool include_equals_overload = false;
            bool include_get_hash_overload = false;
            bool include_reset_class_method = false;

            string class_name = NameFormatter.ToCSharpClassName(sqlTable.Name);
            int longest_column = GetLongestColumnLength(sqlTable);

            var output = new OutputObject();
            output.Name = class_name + ".cs";
            output.Type = OutputObject.eObjectType.CSharp;

            var sb = new StringBuilder();

            #region Header block

            sb.AppendLine("using System;");
            sb.AppendLine("using System.Text;");
            sb.AppendLine();

            // not needed here
            //sb.AppendLine(GenerateNamespaceIncludes(namespace_includes, null));

            #endregion

            sb.AppendLine("namespace " + NameFormatter.ToCSharpPropertyName(sqlTable.Database.Name));
            sb.AppendLine("{");

            //if (includeClassDecoration)
            //    sb.AppendLine(AddTabs(1) + "[SQLTableAttribute(DataBaseName=\"" + sqlTable.Database.Name + "\", TableName=\"" + sqlTable.Name + "\")]");

            sb.AppendLine(AddTabs(1) + "public partial class " + class_name);
            sb.AppendLine(AddTabs(1) + "{");

            #region Constants Bloc
            ////////////////////////////////////////////////////////////////////////////////

            #region sample output
            //#region Constants
            //
            //    public static class Db
            //    {
            //        public static string Id             = "Id";
            //    }
            //
            //#endregion
            #endregion

            if (include_static_field_names)
            {
                if (include_region_blocks)
                {
                    sb.AppendLine(AddTabs(2) + "#region Constants");
                    sb.AppendLine();
                }
                sb.AppendLine(AddTabs(2) + "public static class Db");
                sb.AppendLine(AddTabs(2) + "{");

                foreach (var sql_column in sqlTable.Columns.Values)
                {
                    // format: 
                    // public static string Id = "Id";
                    sb.AppendLine(AddTabs(3) + "public static string " + PadCSharpVariableName(NameFormatter.ToCSharpPropertyName(sql_column.Name), longest_column) + "= \"" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + "\";");
                }

                sb.AppendLine(AddTabs(2) + "}");
                sb.AppendLine();

                if (include_region_blocks)
                {
                    sb.AppendLine(AddTabs(2) + "#endregion");
                    sb.AppendLine();
                }
            }

            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            #region Properties Block
            ////////////////////////////////////////////////////////////////////////////////

            if (include_region_blocks)
            {
                sb.AppendLine(AddTabs(2) + "#region Properties");
                sb.AppendLine();
            }

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                #region Sample Output
                //[SQLColumn(ColumnName = "foo", SQLType = "int", Precision = "4", IsPrimaryKey = "false", IsNullable = "false")]
                //public string SomeID { get; set; }
                //{
                //  get { return _SomeID; }
                //  set { _SomeID = value; }
                //}
                #endregion Sample Output

                if (includeClassDecoration)
                {
                    sb.Append(AddTabs(2) + "[SQLColumnAttribute(");
                    sb.Append("ColumnName=\"" + sql_column.Name + "\", ");
                    sb.Append("SQLType=SqlDbType." + sql_column.DataType + ", ");
                    sb.Append("Length=" + sql_column.Length.ToString() + ", ");
                    sb.Append("IsPrimaryKey=" + sql_column.IsPk.ToString().ToLower() + ", ");
                    sb.Append("IsNullable=" + sql_column.IsNullable.ToString().ToLower());
                    sb.Append(")]" + Environment.NewLine);
                }

                sb.AppendLine(AddTabs(2) + $"public {NameFormatter.SQLTypeToCSharpType(sql_column)} {NameFormatter.ToCSharpPropertyName(sql_column.Name)} {{ get; set; }}");
            }

            if (include_region_blocks)
            {
                sb.AppendLine(AddTabs(2) + "#endregion");
                sb.AppendLine();
            }

            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            if (include_region_blocks)
            {
                sb.AppendLine();
                sb.AppendLine(AddTabs(2) + "#region Methods");
            }

            #region Default CTOR
            ////////////////////////////////////////////////////////////////////////////////

            #region sample output
            //public Foo() {}
            #endregion

            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "public " + NameFormatter.ToCSharpClassName(sqlTable.Name) + "() {}");

            ////////////////////////////////////////////////////////////////////////////////
            #endregion Default CTOR

            #region Reset Method
            //////////////////////////////////////////////////////////////////////////////

            if (include_reset_class_method)
            {
                sb.AppendLine();
                sb.AppendLine(AddTabs(2) + "public void Reset()");
                sb.AppendLine(AddTabs(2) + "{");

                foreach (var sql_column in sqlTable.Columns.Values)
                {
                    // format: 
                    // _State   = string.Empty;
                    sb.Append(AddTabs(3));
                    sb.Append(PadCSharpVariableName(NameFormatter.ToCSharpPrivateVariable(sql_column.Name), longest_column, 0));
                    sb.Append("= " + NameFormatter.GetCSharpDefaultValue(sql_column) + ";" + Environment.NewLine);
                }

                sb.AppendLine(AddTabs(2) + "}");
            }

            //////////////////////////////////////////////////////////////////////////////
            #endregion

            #region ToString Method
            ////////////////////////////////////////////////////////////////////////////////

            if (include_to_string_overload)
            {
                #region Sample Output
                //public override string ToString()
                //{
                //    StringBuilder sb = new StringBuilder();
                //
                //    sb.AppendLine("CountryID:" + _CountryID.ToString());
                //    sb.AppendLine("OfficialName:" + _OfficialName);
                //    sb.AppendLine("CommonName:" + _CommonName);
                //    sb.AppendLine("CapitolName:" + _CapitolName);
                //    sb.AppendLine("Disabled:" + _Disabled.ToString());
                //
                //    return sb.ToString();
                //}
                #endregion

                sb.AppendLine();
                sb.AppendLine(AddTabs(2) + "public override string ToString()");
                sb.AppendLine(AddTabs(2) + "{");
                sb.AppendLine(AddTabs(3) + "StringBuilder sb = new StringBuilder();");
                sb.AppendLine();

                foreach (var sql_column in sqlTable.Columns.Values)
                {
                    // format: 
                    // sb.AppendLine("Foo:" + _Foo.ToString());
                    if (NameFormatter.SQLTypeToCSharpType(sql_column) == "string")
                    {
                        sb.AppendLine(AddTabs(3) + "sb.AppendLine(\"" + sql_column.Name + ": \" + " + NameFormatter.ToCSharpPrivateVariable(sql_column.Name) + " + \" \");");
                    }
                    else
                    {
                        sb.AppendLine(AddTabs(3) + "sb.AppendLine(\"" + sql_column.Name + ": \" + " + NameFormatter.ToCSharpPrivateVariable(sql_column.Name) + ".ToString() + \" \");");
                    }
                }

                sb.AppendLine();
                sb.AppendLine(AddTabs(3) + "return sb.ToString();");
                sb.AppendLine(AddTabs(2) + "}");
            }

            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            #region Equals Method
            ////////////////////////////////////////////////////////////////////////////////

            if (include_equals_overload)
            {
                #region Sample Output
                //public override bool Equals(object obj)
                //{
                //    if (ReferenceEquals(null, obj)) return false;
                //    if (ReferenceEquals(this, obj)) return true;
                //    if (GetType() != obj.GetType()) return false;
                //
                //    cArmor new_obj = (cArmor)obj;     
                //
                //    if (!Object.Equals(this._ArmorID, new_obj._ArmorID)) return false;
                //    if (!Object.Equals(this._ArmorType, new_obj._ArmorType)) return false;
                //    if (!Object.Equals(this._Disabled, new_obj._Disabled)) return false;
                //
                //    return true;
                //}
                #endregion

                sb.AppendLine();
                sb.AppendLine(AddTabs(2) + "public override bool Equals(object obj)");
                sb.AppendLine(AddTabs(2) + "{");
                sb.AppendLine(AddTabs(3) + "if (ReferenceEquals(null, obj)) return false;");
                sb.AppendLine(AddTabs(3) + "if (ReferenceEquals(this, obj)) return true;");
                sb.AppendLine(AddTabs(4) + "if (GetType() != obj.GetType()) return false;");
                sb.AppendLine();

                sb.AppendLine(AddTabs(3) + NameFormatter.ToCSharpClassName(sqlTable.Name) + " new_obj = (" + NameFormatter.ToCSharpClassName(sqlTable.Name) + ")obj;");
                sb.AppendLine();

                foreach (var sql_column in sqlTable.Columns.Values)
                {
                    // format: 
                    // if (!Object.Equals(this._ArmorID, new_obj._ArmorID)) return false;
                    sb.AppendLine(AddTabs(3) + "if (!object.Equals(this." + NameFormatter.ToCSharpPrivateVariable(sql_column.Name) + ", new_obj." + NameFormatter.ToCSharpPrivateVariable(sql_column.Name) + ")) return false;");
                }

                sb.AppendLine();
                sb.AppendLine(AddTabs(3) + "return true;");
                sb.AppendLine(AddTabs(2) + "}");
            }

            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            #region GetHashCode Method
            ////////////////////////////////////////////////////////////////////////////////

            if (include_get_hash_overload)
            {
                #region Sample Output
                //public override int GetHashCode()
                //{
                //    int output = 0;
                //
                //    output ^= (_ArmorID * 2);
                //    output ^= (_ArmorType.GetHashCode() * 3);
                //    output ^= (_Disabled.GetHashCode() * 5);
                //
                //    return output;
                //}
                #endregion

                sb.AppendLine();
                sb.AppendLine(AddTabs(2) + "public override int GetHashCode()");
                sb.AppendLine(AddTabs(2) + "{");
                sb.AppendLine(AddTabs(3) + "unchecked");
                sb.AppendLine(AddTabs(3) + "{");
                sb.AppendLine(AddTabs(4) + "int output = 0;");
                sb.AppendLine();

                int prime = 0;

                foreach (var sql_column in sqlTable.Columns.Values)
                {
                    // format: 
                    // output ^= _ArmorType.GetHashCode();
                    switch (sql_column.SqlDataType)
                    {
                        case SqlDbType.TinyInt:
                        case SqlDbType.SmallInt:
                        case SqlDbType.Int:
                        case SqlDbType.BigInt:
                            sb.AppendLine(AddTabs(4) + $"output ^= ({NameFormatter.ToCSharpPrivateVariable(sql_column.Name)} * {s_PrimeList[prime]});");
                            break;

                        default:
                            sb.AppendLine(AddTabs(4) + $"output ^= ({NameFormatter.ToCSharpPrivateVariable(sql_column.Name)}.GetHashCode() * {s_PrimeList[prime]});");
                            break;
                    }

                    prime++;

                    // my what a wide table you have...
                    if (prime > s_PrimeList.Length)
                        prime = 0;
                }

                sb.AppendLine();
                sb.AppendLine(AddTabs(4) + "return output;");
                sb.AppendLine(AddTabs(3) + "}");
                sb.AppendLine(AddTabs(2) + "}");
            }

            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            if (include_region_blocks)
            {
                sb.AppendLine();
                sb.AppendLine(AddTabs(2) + "#endregion");
            }

            sb.AppendLine(AddTabs(1) + "}");
            sb.AppendLine("}");

            output.Body = sb.ToString();
            return output;
        }
        public static OutputObject GenerateCSharpExternalOrmClass(SqlTable sqlTable, List<string> namespace_includes)
        {
            if (sqlTable == null)
                return null;

            string class_name = NameFormatter.ToCSharpClassName(sqlTable.Name);

            OutputObject output = new OutputObject();
            output.Name = class_name + "_Ext.cs";
            output.Type = OutputObject.eObjectType.CSharp;

            StringBuilder sb = new StringBuilder();

            #region Header block

            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Data;");
            sb.AppendLine("using System.Text;");
            sb.AppendLine();

            // not needed here
            //sb.AppendLine(GenerateNamespaceIncludes(namespace_includes, null));

            #endregion

            sb.AppendLine("namespace " + NameFormatter.ToCSharpPropertyName(sqlTable.Database.Name));
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "public partial class " + class_name);
            sb.AppendLine(AddTabs(1) + "{");
            sb.AppendLine();
            sb.AppendLine(AddTabs(1) + "}");
            sb.AppendLine("}");

            output.Body = sb.ToString();
            return output;
        }
        public static OutputObject GenerateCSharpDalClass(SqlTable sqlTable, List<string> namespace_includes, bool convert_nullable_fields, bool include_is_dirty_flag)
        {
            #region SampleCode
            //public class FooClass
            //{
            //    private List<FooDataStrure> _Foo = null;

            //    //public FooClass()
            //    //{ 
            //    //    // foo
            //    //}

            //    public List<FooDataStrure> LoadData(DataTable dt)
            //    {
            //        _Foo.Clear();  
            //        foreach (DataRow dr in dt.Rows)
            //        {
            //            p_Foo.Add(new FooDataStrure(Convert.ToInt32(dr["whatever"])));
            //        }

            //        return _Foo;
            //    }
            //}
            #endregion

            if (sqlTable == null)
                return null;

            string list_type = NameFormatter.ToCSharpClassName(sqlTable.Name);
            string sql_tablename = NameFormatter.ToTSQLName(sqlTable.Name);
            string collection_type = "List<" + list_type + ">";
            string class_name = NameFormatter.ToCSharpClassName("Dal" + sqlTable.Name);
            int longest_column = GetLongestColumnLength(sqlTable);

            OutputObject output = new OutputObject();
            output.Name = class_name + ".cs";
            output.Type = OutputObject.eObjectType.CSharp;

            StringBuilder sb = new StringBuilder();

            #region Header block

            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Data;");
            sb.AppendLine("using System.Data.SqlClient;");
            sb.AppendLine();

            sb.AppendLine("using DAL;");
            sb.AppendLine(GenerateNamespaceIncludes(namespace_includes));

            #endregion

            sb.AppendLine("namespace " + NameFormatter.ToCSharpPropertyName(sqlTable.Database.Name));
            sb.AppendLine("{");
            //sb.AppendLine(AddTabs(1) + "[Serializable]");
            sb.AppendLine(AddTabs(1) + "public partial class " + class_name);
            sb.AppendLine(AddTabs(1) + "{");

            #region Fields bloc
            ////////////////////////////////////////////////////////////////////////////////

            sb.AppendLine(AddTabs(2) + "#region Fields");
            sb.AppendLine();

            sb.AppendLine(AddTabs(3) + "protected string _SQLConnection;");
            sb.AppendLine();

            sb.AppendLine(AddTabs(2) + "#endregion");
            sb.AppendLine();

            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            sb.AppendLine(AddTabs(2) + "#region Methods");
            sb.AppendLine();

            #region Default CTOR
            ////////////////////////////////////////////////////////////////////////////////

            #region sample output
            //public Foo()
            //{
            //    //foo
            //}
            #endregion

            sb.AppendLine(AddTabs(3) + "public " + class_name + "(string sql_conn)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "_SQLConnection = sql_conn;");
            sb.AppendLine(AddTabs(3) + "}");

            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            sb.AppendLine();
            sb.AppendLine(AddTabs(3) + "// Public Methods");

            #region GetCountFromDb
            ////////////////////////////////////////////////////////////////////////////////

            #region Code Sample
            //public int GetCountFromDb()
            //{
            //    return Database.ExecuteScalarSp<int>("[GalacticConquest].[dbo].[up_Game_CountAll]", null, _SQLConnection);
            //}
            #endregion

            sb.AppendLine(AddTabs(3) + "public int GetCountFromDb()");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "return Database.ExecuteScalarSp<int>(\"[" + NameFormatter.ToCSharpPropertyName(sqlTable.Database.Name) + "].[dbo].[" + GenerateSqlStoredProcName(sqlTable.Name, eStoredProcType.CountAll, null) + "]\", null, _SQLConnection);");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine();

            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            #region GetSearchCountFromDb
            ////////////////////////////////////////////////////////////////////////////////

            #region Code Sample
            //public int GetSearchCountFromDb(string search_string)
            //{
            //    List<SqlParameter> parameters = new List<SqlParameter>();
            //    parameters.Add(new SqlParameter() { ParameterName = "@SearchString", SqlDbType = SqlDbType.VarChar, Size = 50, Value = search_string });
            //    return Database.ExecuteScalarSp<int>("[GalacticConquest].[dbo].[up_Game_CountSearch]", parameters, _SQLConnection);
            //}
            #endregion

            sb.AppendLine(AddTabs(3) + "public int GetSearchCountFromDb(string search_string)");
            sb.AppendLine(AddTabs(3) + "{");

            sb.AppendLine(AddTabs(4) + "List<SqlParameter> parameters = new List<SqlParameter>();");
            sb.AppendLine(AddTabs(4) + string.Format(SQL_PARAMETER_TEMPLATE, "@SearchString", "VarChar", 50, "search_string"));
            sb.AppendLine(AddTabs(4) + "return Database.ExecuteScalarSp<int>(\"" + NameFormatter.ToTSQLName(sqlTable.Database.Name) + ".[dbo].[" + GenerateSqlStoredProcName(sqlTable.Name, eStoredProcType.CountSearch, null) + "]\", parameters, _SQLConnection);");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine();

            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            #region LoadAllFromDb
            ////////////////////////////////////////////////////////////////////////////////

            #region Code Sample
            //public List<cGame> LoadAllFromDb()
            //{
            //    return Database.ExecuteQuerySp<cGame>("[GalacticConquest].[dbo].[up_Game_SelectAll]", null, _SQLConnection);
            //}
            #endregion

            sb.AppendLine(AddTabs(3) + "public " + collection_type + " LoadAllFromDb()");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "return Database.ExecuteQuerySp<" + list_type + ">(\"[" + NameFormatter.ToCSharpPropertyName(sqlTable.Database.Name) + "].[dbo].[" + GenerateSqlStoredProcName(sqlTable.Name, eStoredProcType.SelectAll, null) + "]\", null, _SQLConnection);");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine();

            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            #region LoadListFromDb
            ////////////////////////////////////////////////////////////////////////////////

            #region Code Sample
            //public List<cGame> LoadListFromDb(IEnumerable<int> id_list)
            //{
            //    List<SqlParameter> parameters = new List<SqlParameter>();
            //    parameters.Add(new SqlParameter() { ParameterName = "@IdList", SqlDbType = SqlDbType.VarChar, Size = -1, Value = Database.GenericListToStringList(id_list) });
            //    return Database.ExecuteQuerySp<cGame>("[GalacticConquest].[dbo].[up_Game_SelectMany]", parameters, _SQLConnection);
            //}
            #endregion

            if (sqlTable.PkList.Count == 0)
            {
                sb.AppendLine("// Cannot create LoadListFromDb() without a pk on database table.");
            }
            else
            {
                sb.AppendLine(AddTabs(3) + "public " + collection_type + " LoadListFromDb(IEnumerable<int> id_list)");
                sb.AppendLine(AddTabs(3) + "{");

                sb.AppendLine(AddTabs(4) + "List<SqlParameter> parameters = new List<SqlParameter>();");
                sb.AppendLine(AddTabs(4) + string.Format(SQL_PARAMETER_TEMPLATE, "@IdList", "VarChar", -1, "Database.GenericListToStringList(id_list)"));
                sb.AppendLine(AddTabs(4) + "return Database.ExecuteQuerySp<" + list_type + ">(\"" + NameFormatter.ToTSQLName(sqlTable.Database.Name) + ".[dbo].[" + GenerateSqlStoredProcName(sqlTable.Name, eStoredProcType.SelectMany, null) + "]\", parameters, _SQLConnection);");
                sb.AppendLine(AddTabs(3) + "}");
                sb.AppendLine();
            }

            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            #region LoadAllFromDbPaged
            ////////////////////////////////////////////////////////////////////////////////

            #region Code Sample
            //public List<cGame> LoadAllFromDbPaged(int start_index, int page_size, string search_string)
            //{
            //    List<SqlParameter> parameters = new List<SqlParameter>();
            //    parameters.Add(new SqlParameter() { ParameterName = "@StartNumber", SqlDbType = SqlDbType.Int, Size = 4, Value = start_index });
            //    parameters.Add(new SqlParameter() { ParameterName = "@PageSize", SqlDbType = SqlDbType.Int, Size = 4, Value = page_size });
            //    parameters.Add(new SqlParameter() { ParameterName = "@SearchString", SqlDbType = SqlDbType.VarChar, Size = 50, Value = search_string });
            //    return Database.ExecuteQuerySp<cGame>("[GalacticConquest].[dbo].[up_Game_SelectAllPaged]", parameters, _SQLConnection);
            //}
            #endregion

            sb.AppendLine(AddTabs(3) + "public " + collection_type + " LoadAllFromDbPaged(int start_index, int page_size, string search_string)");
            sb.AppendLine(AddTabs(3) + "{");

            sb.AppendLine(AddTabs(4) + "List<SqlParameter> parameters = new List<SqlParameter>();");
            sb.AppendLine(AddTabs(4) + string.Format(SQL_PARAMETER_TEMPLATE, "@StartNumber", "Int", 4, "start_index"));
            sb.AppendLine(AddTabs(4) + string.Format(SQL_PARAMETER_TEMPLATE, "@PageSize", "Int", 4, "page_size"));
            sb.AppendLine(AddTabs(4) + string.Format(SQL_PARAMETER_TEMPLATE, "@SearchString", "VarChar", 50, "search_string"));
            sb.AppendLine(AddTabs(4) + "return Database.ExecuteQuerySp<" + list_type + ">(\"[" + NameFormatter.ToCSharpPropertyName(sqlTable.Database.Name) + "].[dbo].[" + GenerateSqlStoredProcName(sqlTable.Name, eStoredProcType.SelectAllPag, null) + "]\", parameters, _SQLConnection);");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine();

            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            #region LoadSingleFromDb
            ////////////////////////////////////////////////////////////////////////////////

            #region Code Sample
            //public cGame LoadSingleFromDb(int id)
            //{
            //    List<SqlParameter> parameters = new List<SqlParameter>();
            //    parameters.Add(new SqlParameter() { ParameterName = "@Id", SqlDbType = SqlDbType.Int, Size = 4, Value = id });
            //    var results = Database.ExecuteQuerySp<cGame>("[GalacticConquest].[dbo].[up_Game_SelectSingle]", parameters, _SQLConnection);

            //    if (results != null && results.Count > 0)
            //        return results[0];
            //    else
            //        return null;
            //}
            #endregion

            sb.AppendLine(AddTabs(3) + "public " + list_type + " LoadSingleFromDb(" + NameFormatter.GenerateCSharpFunctionArgs(sqlTable, eIncludedFields.PKOnly) + ")");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "List<SqlParameter> parameters = new List<SqlParameter>();");

            // generate all Sp parameters
            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if (sql_column.IsPk)
                    sb.AppendLine(AddTabs(4) + string.Format(SQL_PARAMETER_TEMPLATE, NameFormatter.ToTSQLVariableName(sql_column), sql_column.SqlDataType, sql_column.Length, NameFormatter.ToCSharpLocalVariable(sql_column.Name)));
            }

            sb.AppendLine(AddTabs(4) + "var results = Database.ExecuteQuerySp<" + list_type + ">(\"[" + NameFormatter.ToCSharpPropertyName(sqlTable.Database.Name) + "].[dbo].[" + GenerateSqlStoredProcName(sqlTable.Name, eStoredProcType.SelectSingle, null) + "]\", parameters, _SQLConnection);");
            sb.AppendLine();

            sb.AppendLine(AddTabs(4) + "if (results != null && results.Count > 0)");
            sb.AppendLine(AddTabs(5) + "return results[0];");
            sb.AppendLine(AddTabs(4) + "else");
            sb.AppendLine(AddTabs(5) + "return null;");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine();

            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            #region DeleteSingleFromDb
            ////////////////////////////////////////////////////////////////////////////////

            #region Code Sample
            //public bool DeleteSingleFromDb(int id)
            //{
            //    List<SqlParameter> parameters = new List<SqlParameter>();
            //    parameters.Add(new SqlParameter() { ParameterName = "@Id", SqlDbType = SqlDbType.Int, Size = 4, Value = id });
            //    return Database.ExecuteNonQuerySp("[GalacticConquest].[dbo].[up_Game_DeleteSingle]", parameters, _SQLConnection) > 0;
            //}
            #endregion

            sb.AppendLine(AddTabs(3) + "public bool DeleteSingleFromDb(" + NameFormatter.GenerateCSharpFunctionArgs(sqlTable, eIncludedFields.PKOnly) + ")");
            sb.AppendLine(AddTabs(3) + "{");

            sb.AppendLine(AddTabs(4) + "List<SqlParameter> parameters = new List<SqlParameter>();");

            // generate all SP parameters
            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if (sql_column.IsPk)
                    sb.AppendLine(AddTabs(4) + string.Format(SQL_PARAMETER_TEMPLATE, NameFormatter.ToTSQLVariableName(sql_column), sql_column.SqlDataType, sql_column.Length, NameFormatter.ToCSharpLocalVariable(sql_column.Name)));
            }

            sb.AppendLine(AddTabs(4) + "return Database.ExecuteNonQuerySp(\"" + NameFormatter.ToTSQLName(sqlTable.Database.Name) + ".[dbo].[" + GenerateSqlStoredProcName(sqlTable.Name, eStoredProcType.DelSingle, null) + "]\", parameters, _SQLConnection) > 0;");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine();

            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            #region DeleteListFromDb
            ////////////////////////////////////////////////////////////////////////////////

            #region Code Sample
            //public int DeleteListFromDb(IEnumerable<int> id_list)
            //{
            //    List<SqlParameter> parameters = new List<SqlParameter>();
            //    parameters.Add(new SqlParameter() { ParameterName = "@IdList", SqlDbType = SqlDbType.VarChar, Size = -1, Value = Database.GenericListToStringList<int>(id_list) });
            //    return Database.ExecuteNonQuerySp("[GalacticConquest].[dbo].[up_Game_DeleteMany]", parameters, _SQLConnection);
            //}
            #endregion

            sb.AppendLine(AddTabs(3) + "public int DeleteListFromDb(IEnumerable<int> id_list)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "List<SqlParameter> parameters = new List<SqlParameter>();");
            sb.AppendLine(AddTabs(4) + string.Format(SQL_PARAMETER_TEMPLATE, "@IdList", "VarChar", -1, "Database.GenericListToStringList(id_list)"));
            sb.AppendLine(AddTabs(4) + "return Database.ExecuteNonQuerySp(\"" + NameFormatter.ToTSQLName(sqlTable.Database.Name) + ".[dbo].[" + GenerateSqlStoredProcName(sqlTable.Name, eStoredProcType.DelMany, null) + "]\", parameters, _SQLConnection);");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine();

            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            #region DeleteAllFromDb
            ////////////////////////////////////////////////////////////////////////////////

            #region Code Sample
            //public int DeleteAllFromDb()
            //{
            //    return Database.ExecuteNonQuerySp("[GalacticConquest].[dbo].[up_Game_DeleteAll]", null, _SQLConnection);
            //}
            #endregion

            sb.AppendLine(AddTabs(3) + "public int DeleteAllFromDb()");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "return Database.ExecuteNonQuerySp(\"" + NameFormatter.ToTSQLName(sqlTable.Database.Name) + ".[dbo].[" + GenerateSqlStoredProcName(sqlTable.Name, eStoredProcType.DelAll, null) + "]\", null, _SQLConnection);");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine();

            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            #region LoadFromXmlFile
            ////////////////////////////////////////////////////////////////////////////////

            #region Code Sample
            //public bool LoadFromXmlFile(string filename)
            //{
            //    string file_data = FileIo.ReadFromFile(filename, out file_data);
            //    DataSet ds = XmlConverter.XmlStringToDataSet(file_data);
            //    return LoadFromDataTable(ds.Tables["Game"], out results, out exception);
            //}
            #endregion

            sb.AppendLine(AddTabs(3) + "public " + collection_type + " LoadFromXmlFile(string filename)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "string file_data = FileIo.ReadFromFile(filename);");
            sb.AppendLine(AddTabs(4) + "DataSet ds = XmlConverter.XmlStringToDataSet(file_data);");
            sb.AppendLine(AddTabs(4) + "return LoadFromDataTable(ds.Tables[\"" + sqlTable.Name + "\"]);");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine();

            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            #region SaveToXmlFile
            ////////////////////////////////////////////////////////////////////////////////

            #region Code Sample
            //public void SaveToXmlFile(List<cGame> collection, string filename)
            //{
            //    DataTable dt = ConvertToDataTable(collection);
            //    string xml = XmlConverter.DataTableToXmlString(dt, "GalacticConquest");
            //    FileIo.WriteToFile(filename, xml);
            //}
            #endregion

            sb.AppendLine(AddTabs(3) + "public void SaveToXmlFile(" + collection_type + " collection, string filename)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "DataTable dt = ConvertToDataTable(collection);");
            sb.AppendLine(AddTabs(4) + "string xml = XmlConverter.DataTableToXmlString(dt, \"" + NameFormatter.ToCSharpPropertyName(sqlTable.Database.Name) + "\");");
            sb.AppendLine(AddTabs(4) + "FileIo.WriteToFile(filename, xml);");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine();

            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            #region SaveAll Method
            ////////////////////////////////////////////////////////////////////////////////

            #region Code Sample
            //public void SaveAll(List<cGame> collection)
            //{
            //    foreach (var item in collection)
            //        Save(item);
            //}
            #endregion

            sb.AppendLine(AddTabs(3) + "public void SaveAll(" + collection_type + " collection)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "foreach (var item in collection)");
            sb.AppendLine(AddTabs(5) + "Save(item);");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine();

            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            #region Save Method
            ////////////////////////////////////////////////////////////////////////////////

            #region Code Sample
            //public bool Save(cGame item)
            //{
            //    List<SqlParameter> parameters = GetSqlParameterList(item);
            //    return Database.ExecuteNonQuerySp("[GalacticConquest].[dbo].[up_Game_Set]", parameters, _SQLConnection) > 0;
            //}
            #endregion

            sb.AppendLine(AddTabs(3) + "public bool Save(" + list_type + " item)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "var parameters = GetSqlParameterList(item);");
            sb.AppendLine(AddTabs(4) + "return Database.ExecuteNonQuerySp(\"" + NameFormatter.ToTSQLName(sqlTable.Database.Name) + ".[dbo].[" + GenerateSqlStoredProcName(sqlTable.Name, eStoredProcType.UpdateInsert, null) + "]\", parameters, _SQLConnection) > 0;");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine();

            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            #region LoadFromDataTable
            ////////////////////////////////////////////////////////////////////////////////

            #region Code Sample
            //protected List<cGame> LoadFromDataTable(DataTable dt)
            //{
            //    if (dt == null || dt.Rows.Count < 1 || dt.Columns.Count < 1)
            //        return null;

            //    var results = new List<cGame>();

            //    foreach (DataRow dr in dt.Rows)
            //    {
            //        cGame obj = new cGame();

            //        obj.Id = Convert.ToInt32(dr["Id"]);
            //        obj.Name = Convert.ToString(dr["Name"]);
            //        obj.StarDate = Convert.ToDouble(dr["StarDate"]);
            //        obj.GameStateId = Convert.ToInt32(dr["GameStateId"]);
            //        obj.CreatedById = Convert.ToInt32(dr["CreatedById"]);
            //        obj.CreatedDate = Convert.ToDateTime(dr["CreatedDate"]);
            //        obj.Description = Convert.ToString(dr["Description"]);
            //        obj.Disabled = Convert.ToBoolean(dr["Disabled"]);

            //        results.Add(obj);
            //    }

            //    return results;
            //}
            #endregion

            sb.AppendLine(AddTabs(3) + "protected " + collection_type + " LoadFromDataTable(DataTable dt)");
            sb.AppendLine(AddTabs(3) + "{");

            sb.AppendLine(AddTabs(4) + "if (dt == null || dt.Rows.Count < 1 || dt.Columns.Count < 1)");
            sb.AppendLine(AddTabs(5) + "return null;");
            sb.AppendLine();

            sb.AppendLine(AddTabs(4) + "var results = new " + collection_type + "();");
            sb.AppendLine();

            sb.AppendLine(AddTabs(4) + "foreach (DataRow dr in dt.Rows)");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + PadCSharpVariableName(NameFormatter.ToCSharpClassName(sqlTable.Name) + " obj", longest_column) + "= new " + NameFormatter.ToCSharpClassName(sqlTable.Name) + "();");
            sb.AppendLine();

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                sb.Append(AddTabs(5));

                if (sql_column.IsNullable)
                {
                    // hack - nullable doubles need special treatment
                    switch (sql_column.SqlDataType)
                    {
                        case SqlDbType.Float:

                            sb.Append(PadCSharpVariableName("obj." + NameFormatter.ToCSharpPropertyName(sql_column.Name), longest_column));
                            sb.Append("= (dr[\"" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + "\"] == DBNull.Value) ? null : ");
                            sb.Append("new double?(" + NameFormatter.GetCSharpCastString(sql_column) + "(dr[\"" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + "\"]));");
                            break;

                        case SqlDbType.UniqueIdentifier:

                            sb.Append(PadCSharpVariableName("obj." + NameFormatter.ToCSharpPropertyName(sql_column.Name), longest_column));
                            sb.Append("= (dr[\"" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + "\"] == DBNull.Value) ? null : ");
                            sb.Append("new Guid?(new Guid((string)dr[\"" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + "\"]));");
                            break;

                        case SqlDbType.DateTime:

                            sb.Append(PadCSharpVariableName("obj." + NameFormatter.ToCSharpPropertyName(sql_column.Name), longest_column));
                            sb.Append("= (dr[\"" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + "\"] == DBNull.Value) ? null : ");
                            sb.Append("new DateTime?(Convert.ToDateTime(dr[\"" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + "\"]));");
                            break;

                        // TODO may need more switch cases here...

                        default:

                            sb.Append(PadCSharpVariableName("obj." + NameFormatter.ToCSharpPropertyName(sql_column.Name), longest_column));
                            sb.Append("= (dr[\"" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + "\"] == DBNull.Value) ? null : ");
                            sb.Append(NameFormatter.GetCSharpCastString(sql_column) + "(dr[\"" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + "\"]);");
                            break;
                    }
                }
                else
                {
                    switch (sql_column.SqlDataType)
                    {
                        case SqlDbType.UniqueIdentifier:

                            sb.Append(PadCSharpVariableName("obj." + NameFormatter.ToCSharpPropertyName(sql_column.Name), longest_column));
                            sb.Append("= new Guid((string)(dr[\"" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + "\"]));");
                            break;

                        default:

                            sb.Append(PadCSharpVariableName("obj." + NameFormatter.ToCSharpPropertyName(sql_column.Name), longest_column));
                            sb.Append("= " + NameFormatter.GetCSharpCastString(sql_column) + "(dr[\"" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + "\"]);");
                            break;
                    }
                }

                sb.AppendLine();
            }
            sb.AppendLine();

            sb.AppendLine(AddTabs(5) + "results.Add(obj);");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "return results;");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine();

            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            #region ConvertToDataTable
            ////////////////////////////////////////////////////////////////////////////////

            #region Code Sample
            //protected DataTable ConvertToDataTable(List<cGame> collection)
            //{
            //    DataTable dt = new DataTable();

            //    dt.Columns.Add("Id", typeof(int));
            //    dt.Columns.Add("Name", typeof(string));
            //    dt.Columns.Add("Disabled", typeof(bool));

            //    dt.TableName = "Game";

            //    foreach (var obj in collection)
            //    {
            //        DataRow dr = dt.NewRow();

            //        dr["Id"]            = obj.Id;
            //        dr["Name"]          = obj.Name;
            //        dr["Disabled"]      = obj.Disabled;

            //        dt.Rows.Add(dr);
            //    }

            //    return dt;
            //}
            #endregion

            sb.AppendLine(AddTabs(3) + "protected DataTable ConvertToDataTable(" + collection_type + " collection)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "DataTable dt = new DataTable();");
            sb.AppendLine();

            // dt.Columns.Add("Name", typeof(string));
            foreach (var sql_column in sqlTable.Columns.Values)
                sb.AppendLine(AddTabs(4) + "dt.Columns.Add(\"" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + "\", typeof(" + NameFormatter.SQLTypeToCSharpType(sql_column) + "));");

            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "dt.TableName = \"" + sqlTable.Name + "\";");
            sb.AppendLine();

            sb.AppendLine(AddTabs(4) + "foreach (var obj in collection)");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "DataRow dr = dt.NewRow();");
            sb.AppendLine();

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                sb.Append(AddTabs(5));
                sb.Append(PadCSharpVariableName("dr[\"" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + "\"]", longest_column + 7));
                sb.Append("= obj." + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ";" + Environment.NewLine);
            }

            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "dt.Rows.Add(dr);");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "return dt;");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine();

            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            #region GetSqlParameterList Method
            ////////////////////////////////////////////////////////////////////////////////

            #region Code Sample
            //protected SqlParameter[] GetSqlParameterList(cRace obj)
            //{
            //    return new SqlParameter[]
            //    {
            //            new SqlParameter() { ParameterName = "AccountId", SqlDbType = SqlDbType.Int, Value = accountId },
            //    };
            //}
            #endregion

            sb.AppendLine(AddTabs(3) + "protected SqlParameter[] GetSqlParameterList(" + NameFormatter.ToCSharpClassName(sqlTable.Name) + " obj)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "return new SqlParameter[]");
            sb.AppendLine(AddTabs(4) + "{");

            // build parameter collection here
            foreach (var sql_column in sqlTable.Columns.Values)
                sb.AppendLine(AddTabs(5) + NameFormatter.ToCSharpSQLParameterTypeString(sql_column, convert_nullable_fields));

            sb.AppendLine(AddTabs(4) + "};");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine();

            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            sb.AppendLine(AddTabs(2) + "#endregion");

            sb.AppendLine(AddTabs(1) + "}");
            sb.AppendLine("}");

            output.Body = sb.ToString();

            return output;
        }
        public static OutputObject GenerateCSharpExternalDalClass(SqlTable sqlTable, List<string> namespace_includes)
        {
            #region SampleCode
            //public partial class FooClass
            //{
            //}
            #endregion

            if (sqlTable == null)
                return null;

            string class_name = NameFormatter.ToCSharpClassName("Dal" + sqlTable.Name);

            OutputObject output = new OutputObject();
            output.Name = class_name + "_Ext.cs";
            output.Type = OutputObject.eObjectType.CSharp;

            StringBuilder sb = new StringBuilder();

            #region Header block

            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            //sb.AppendLine("using System.Configuration;");
            sb.AppendLine("using System.Data;");
            sb.AppendLine("using System.Data.SqlClient;");
            //sb.AppendLine("using System.Linq;");
            //sb.AppendLine("using System.Text;");
            sb.AppendLine();

            sb.AppendLine("using DAL;");
            sb.AppendLine(GenerateNamespaceIncludes(namespace_includes));

            #endregion

            sb.AppendLine("namespace " + NameFormatter.ToCSharpPropertyName(sqlTable.Database.Name));
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "public partial class " + class_name);
            sb.AppendLine(AddTabs(1) + "{");
            sb.AppendLine();
            sb.AppendLine(AddTabs(1) + "}");
            sb.AppendLine("}");

            output.Body = sb.ToString();

            return output;
        }
        public static OutputObject GenerateCSharpClassInterface(SqlTable sqlTable, List<string> namespace_includes, bool include_is_dirty_flag)
        {
            if (sqlTable == null)
                return null;

            string interface_name = NameFormatter.ToCSharpInterfaceName(sqlTable.Name);

            OutputObject output = new OutputObject();
            output.Name = interface_name + ".cs";
            output.Type = OutputObject.eObjectType.CSharp;

            #region SampleCode
            //public interface ICar
            //{
            //    void StartEngine();
            //    bool IsMoving { get; }
            //}
            #endregion

            StringBuilder sb = new StringBuilder();

            #region Header block

            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Data;");
            sb.AppendLine();

            sb.AppendLine(GenerateNamespaceIncludes(namespace_includes));

            #endregion

            sb.AppendLine("namespace " + NameFormatter.ToCSharpPropertyName(sqlTable.Database.Name));
            sb.AppendLine("{");

            sb.AppendLine(AddTabs(1) + "public interface " + interface_name);
            sb.AppendLine(AddTabs(1) + "{");

            #region Properties Block
            ////////////////////////////////////////////////////////////////////////////////
            sb.AppendLine(AddTabs(2) + "#region Properties");
            sb.AppendLine();

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                // Sample: string SomeID { get; set; }
                sb.AppendLine(AddTabs(3) + NameFormatter.SQLTypeToCSharpType(sql_column) + " " + NameFormatter.ToCSharpPropertyName(sql_column.Name) + " { get; set; }");
            }

            if (include_is_dirty_flag)
            {
                sb.AppendLine();
                sb.AppendLine(AddTabs(3) + "bool IsDirty { get; set; }");
            }

            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#endregion");

            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            sb.AppendLine(AddTabs(1) + "}");
            sb.AppendLine("}");

            output.Body = sb.ToString();
            return output;
        }
        public static OutputObject GenerateCSharpEnumeration(SqlTable sqlTable, string string_column, string value_column, DataTable data_table)
        {
            if (sqlTable == null)
                return null;

            string enum_name = NameFormatter.ToCSharpEnumName(sqlTable.Name);
            int longest_column = GetLongestColumnLength(sqlTable);

            OutputObject output = new OutputObject();
            output.Name = enum_name + ".cs";
            output.Type = OutputObject.eObjectType.CSharp;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("using System;");
            sb.AppendLine();

            sb.AppendLine("namespace " + NameFormatter.ToCSharpPropertyName(sqlTable.Database.Name));
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "public enum " + NameFormatter.ToCSharpEnumName(sqlTable.Name));
            sb.AppendLine(AddTabs(1) + "{");

            // list enumeration
            foreach (DataRow dr in data_table.Rows)
            {
                string value_string = dr[value_column].ToString();
                string enum_string = dr[string_column].ToString();

                // make sure that enums are legal names.
                foreach (string character in s_CSharpUndesireables)
                    enum_string = enum_string.Replace(character, string.Empty);

                // no leading numbers, foo!
                if (Char.IsNumber(enum_string[0]))
                    enum_string = "N" + enum_string;

                sb.AppendLine(AddTabs(2) + PadCSharpVariableName(enum_string, longest_column) + "= " + value_string + ",");
            }

            sb.AppendLine(AddTabs(1) + "}");
            sb.AppendLine("}");

            output.Body = sb.ToString();
            return output;
        }
        public static OutputObject GenerateCSharpBaseClass(string database_name)
        {
            if (string.IsNullOrEmpty(database_name))
                return null;

            OutputObject output = new OutputObject();
            output.Name = "BaseClass.cs";
            output.Type = OutputObject.eObjectType.CSharp;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("using System;");
            sb.AppendLine("using System.Text;");
            sb.AppendLine();
            sb.AppendLine("namespace " + database_name);
            sb.AppendLine("{");

            sb.AppendLine(AddTabs(1) + "[Serializable]");
            sb.AppendLine(AddTabs(1) + "public abstract class BaseClass");
            sb.AppendLine(AddTabs(1) + "{");

            sb.AppendLine(AddTabs(2) + "#region Fields");
            sb.AppendLine();
            sb.AppendLine(AddTabs(3) + "protected bool _IsDirty = false;");
            sb.AppendLine(AddTabs(3) + "protected bool _LoadError = false;");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#endregion");
            sb.AppendLine();

            sb.AppendLine(AddTabs(2) + "#region Properties");
            sb.AppendLine();
            sb.AppendLine(AddTabs(3) + "public bool IsDirty");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "get { return _IsDirty; }");
            sb.AppendLine(AddTabs(4) + "set { _IsDirty = value; }");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine(AddTabs(3) + "public bool LoadError");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "get { return _LoadError; }");
            sb.AppendLine(AddTabs(4) + "set { _LoadError = value; }");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#endregion");
            sb.AppendLine();

            sb.AppendLine(AddTabs(2) + "#region Methods");
            sb.AppendLine();

            sb.AppendLine(AddTabs(3) + "public virtual void Reset()");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "_IsDirty = false;");
            sb.AppendLine(AddTabs(4) + "_LoadError = false;");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine();

            sb.AppendLine(AddTabs(3) + "public override string ToString()");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "StringBuilder sb = new StringBuilder();");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "sb.AppendLine(\"_IsDirty: \" + _IsDirty.ToString());");
            sb.AppendLine(AddTabs(4) + "sb.AppendLine(\"_LoadError: \" + _LoadError.ToString());");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "return sb.ToString();");
            sb.AppendLine(AddTabs(3) + "}");

            sb.AppendLine(AddTabs(2) + "#endregion");

            sb.AppendLine(AddTabs(1) + "}");
            sb.AppendLine("}");

            output.Body = sb.ToString();
            return output;
        }

        // WinForm Generation
        public static OutputObject GenerateWinformEditCode(SqlTable sqlTable)
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

            StringBuilder sb = new StringBuilder();

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
        public static OutputObject GenerateWinformEditCodeDesigner(SqlTable sqlTable)
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

            StringBuilder sb = new StringBuilder();

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
        public static OutputObject GenerateWinformViewCode(SqlTable sqlTable)
        {
            return null;
        }
        public static OutputObject GenerateWinformViewCodeDesigner(SqlTable sqlTable)
        {
            return null;
        }
        public static OutputObject GenerateWinformMainCode(SqlTable sqlTable)
        {
            return null;
        }
        public static OutputObject GenerateWinformMainCodeDesigner(SqlTable sqlTable)
        {
            return null;
        }

        // Webservice Generation
        public static OutputObject GenerateWebServiceCodeInfrontClass(SqlTable sqlTable)
        {
            if (sqlTable == null)
                return null;

            string class_name = NameFormatter.ToCSharpClassName(sqlTable.Name);

            OutputObject output = new OutputObject();
            output.Name = class_name + ".asmx";
            output.Type = OutputObject.eObjectType.WebService;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<%@ WebService Language=\"C#\" CodeBehind=\"Service1.asmx.cs\" Class=\"WebService1.Service1\" %>");

            output.Body = sb.ToString();
            return output;
        }
        public static OutputObject GenerateWebServiceCodeBehindClass(SqlTable sqlTable, List<string> namespace_includes)
        {
            if (sqlTable == null)
                return null;

            string class_name = NameFormatter.ToCSharpClassName(sqlTable.Name);

            OutputObject output = new OutputObject();
            output.Name = class_name + ".asmx.cs";
            output.Type = OutputObject.eObjectType.CSharp;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("using System;");
            sb.AppendLine("using System.Data;");
            sb.AppendLine("using System.Data;");
            sb.AppendLine("using System.Web;");
            sb.AppendLine("using System.Collections;");
            sb.AppendLine("using System.Web.Services;");
            sb.AppendLine("using System.Web.Services.Protocols;");
            sb.AppendLine("using System.ComponentModel;");
            sb.AppendLine();

            sb.AppendLine(GenerateNamespaceIncludes(namespace_includes));
            sb.AppendLine();

            sb.AppendLine("namespace WebService1");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "[WebService(Namespace = \"http://tempuri.org/\")]");
            sb.AppendLine(AddTabs(1) + "[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]");
            sb.AppendLine(AddTabs(1) + "[ToolboxItem(false)]");
            sb.AppendLine(AddTabs(1) + "public class Service1 : System.Web.Services.WebService");
            sb.AppendLine(AddTabs(1) + "{");
            sb.AppendLine(AddTabs(2) + "[WebMethod]");
            sb.AppendLine(AddTabs(2) + "public string HelloWorld()");
            sb.AppendLine(AddTabs(2) + "{");
            sb.AppendLine(AddTabs(3) + "return \"Hello World\";");
            sb.AppendLine(AddTabs(2) + "}");
            sb.AppendLine(AddTabs(1) + "}");
            sb.AppendLine("}");

            output.Body = sb.ToString();
            return output;
        }

        // Asp.Net Generation
        public static OutputObject GenerateCSSSheet(string database_name)
        {
            if (string.IsNullOrEmpty(database_name))
                return null;

            OutputObject output = new OutputObject();
            output.Name = ConfigurationManager.AppSettings["DefaultCSSFilename"];
            output.Type = OutputObject.eObjectType.Css;

            string spacer = "/*******************************************************************************/";

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(GenerateHeader(database_name));
            sb.AppendLine();

            //////////////////////////////////////////////////////////////////////////////
            sb.AppendLine(spacer);
            sb.AppendLine();

            sb.AppendLine("#header");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "padding: 10px;");
            sb.AppendLine(AddTabs(1) + "background-color: #00ff00;");
            sb.AppendLine(AddTabs(1) + "border-bottom: solid 2px black;");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine("#menu");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "float: left;");
            sb.AppendLine(AddTabs(1) + "padding-top: 10px;");
            sb.AppendLine(AddTabs(1) + "padding-left: 10px;");
            sb.AppendLine(AddTabs(1) + "padding-right: 50px;");
            sb.AppendLine(AddTabs(1) + "padding-bottom: 200px;");
            sb.AppendLine(AddTabs(1) + "background-color: #ffff00;");
            sb.AppendLine(AddTabs(1) + "border-bottom: solid 2px black;");
            sb.AppendLine(AddTabs(1) + "border-right: solid 2px black;");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine("#contents");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "float: left;");
            sb.AppendLine(AddTabs(1) + "padding: 10px;");
            sb.AppendLine(AddTabs(1) + "background-color: white;");
            sb.AppendLine("}");
            sb.AppendLine();

            //////////////////////////////////////////////////////////////////////////////
            sb.AppendLine(spacer);
            sb.AppendLine();

            sb.AppendLine("HTML");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "height: 100%;");
            sb.AppendLine(AddTabs(1) + "padding: 0px;");
            sb.AppendLine(AddTabs(1) + "border: 0px;");
            sb.AppendLine(AddTabs(1) + "margin: 0px;");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine("Body");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "height: 100%;");
            sb.AppendLine(AddTabs(1) + "padding: 0px;");
            sb.AppendLine(AddTabs(1) + "border: 0px;");
            sb.AppendLine(AddTabs(1) + "margin: 0px;");
            sb.AppendLine(AddTabs(1) + "font-family: Arial, Sans-Serif;");
            sb.AppendLine(AddTabs(1) + "font-size: 12px;");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine("Table");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "border-collapse: collapse;");
            sb.AppendLine(AddTabs(1) + "empty-cells: show;");
            sb.AppendLine(AddTabs(1) + "font-family: arial;");
            sb.AppendLine(AddTabs(1) + "font-size: inherit;");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine("TD");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "margin: 0px;");
            sb.AppendLine(AddTabs(1) + "vertical-align: middle;");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine("Input");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "font-family: arial;");
            sb.AppendLine(AddTabs(1) + "font-size: inherit;");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine("TextArea");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "font-family: arial;");
            sb.AppendLine(AddTabs(1) + "font-size: inherit;");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine("CheckBox");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "font-family: arial;");
            sb.AppendLine(AddTabs(1) + "font-size: inherit;");
            sb.AppendLine("}");
            sb.AppendLine();

            //////////////////////////////////////////////////////////////////////////////
            sb.AppendLine(spacer);
            sb.AppendLine();

            sb.AppendLine(".PageTitle");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "font-size: 14px;");
            sb.AppendLine(AddTabs(1) + "font-weight: bold;");
            sb.AppendLine(AddTabs(1) + "padding-right: 10px;");
            sb.AppendLine(AddTabs(1) + "padding-top: 10px;");
            sb.AppendLine(AddTabs(1) + "padding-bottom: 10px;");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine(".SectionTitle");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "font-size: 12px;");
            sb.AppendLine(AddTabs(1) + "font-weight: bold;");
            sb.AppendLine(AddTabs(1) + "padding-bottom: 10px;");
            sb.AppendLine(AddTabs(1) + "padding-top: 10px;");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine(".ControlLabel");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "font-weight: bold;");
            sb.AppendLine(AddTabs(1) + "font-size: 10pt;");
            sb.AppendLine(AddTabs(1) + "padding-top: 4px;");
            sb.AppendLine(AddTabs(1) + "padding-bottom: 4px;");
            sb.AppendLine(AddTabs(1) + "padding-right: 4px;");
            sb.AppendLine(AddTabs(1) + "text-align: right;");
            sb.AppendLine(AddTabs(1) + "vertical-align:top;");
            sb.AppendLine("}");
            sb.AppendLine();

            //////////////////////////////////////////////////////////////////////////////
            sb.AppendLine(spacer);
            sb.AppendLine();

            sb.AppendLine(".RepeaterStyle");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "border-style: solid;");
            sb.AppendLine(AddTabs(1) + "border-width: 2px;");
            sb.AppendLine(AddTabs(1) + "background-color: #eeeeee;");
            sb.AppendLine(AddTabs(1) + "height: 400px;");
            sb.AppendLine(AddTabs(1) + "width: 800px;");
            sb.AppendLine(AddTabs(1) + "overflow: auto;");
            sb.AppendLine("}");
            sb.AppendLine("@media print");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + ".RepeaterStyle");
            sb.AppendLine(AddTabs(1) + "{");
            sb.AppendLine(AddTabs(2) + "overflow: visible;");
            sb.AppendLine(AddTabs(1) + "}");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine(".RepeaterHeader");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "font-weight: bold;");
            sb.AppendLine(AddTabs(1) + "font-size: 10pt;");
            sb.AppendLine(AddTabs(1) + "padding-bottom: 2px;");
            sb.AppendLine(AddTabs(1) + "padding-top: 4px;");
            sb.AppendLine(AddTabs(1) + "padding-right: 2px;");
            sb.AppendLine(AddTabs(1) + "padding-left: 4px;");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine(".RepeaterCell");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "padding-bottom: 2px;");
            sb.AppendLine(AddTabs(1) + "padding-top: 4px;");
            sb.AppendLine(AddTabs(1) + "padding-right: 2px;");
            sb.AppendLine(AddTabs(1) + "padding-left: 4px;");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine(".RepeaterHeaderStyle");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "border-style: solid;");
            sb.AppendLine(AddTabs(1) + "border-width: 2px 2px 0px 2px;");
            sb.AppendLine(AddTabs(1) + "background-color: #ccccff;");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine(".RepeaterItemStyle");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "background-color: #ffffff;");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine(".AlternatingRepeaterItemStyle");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "background-color: #ffffcc;");
            sb.AppendLine("}");
            sb.AppendLine();

            //////////////////////////////////////////////////////////////////////////////
            sb.AppendLine(spacer);
            sb.AppendLine();

            sb.AppendLine(".DetailsPanel");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "border-style: solid;");
            sb.AppendLine(AddTabs(1) + "border-width: 2px;");
            sb.AppendLine(AddTabs(1) + "background-color: #eeeeee;");
            sb.AppendLine(AddTabs(1) + "width: 600px;");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine(".Spacer");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "padding: 10px;");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine(".AddItemLink");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "padding-top: 10px;");
            sb.AppendLine(AddTabs(1) + "padding-right: 10px;");
            sb.AppendLine(AddTabs(1) + "padding-left: 10px;");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine(".ErrorMessage");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "font-weight: bold;");
            sb.AppendLine(AddTabs(1) + "font-size: inherit;");
            sb.AppendLine(AddTabs(1) + "color: #cc0000;");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine(".button1");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "border: #000000 1px solid;");
            sb.AppendLine(AddTabs(1) + "font-size: 12px;");
            sb.AppendLine(AddTabs(1) + "cursor: hand;");
            sb.AppendLine(AddTabs(1) + "color: #000000;");
            sb.AppendLine(AddTabs(1) + "background-color: #ccccff;");
            sb.AppendLine("}");
            sb.AppendLine();

            //////////////////////////////////////////////////////////////////////////////
            sb.AppendLine(spacer);
            sb.AppendLine();

            output.Body = sb.ToString();
            return output;
        }

        public static OutputObject GenerateWebConfig(ConnectionString connection_string)
        {
            if (connection_string == null)
                return null;

            var output = new OutputObject();
            output.Name = ConfigurationManager.AppSettings["DefaultWebConfigFilename"];
            output.Type = OutputObject.eObjectType.WebConfig;

            var sb = new StringBuilder();

            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            sb.AppendLine("<configuration>");
            sb.AppendLine();
            sb.AppendLine(AddTabs(1) + "<appSettings>");
            sb.AppendLine(AddTabs(1) + "</appSettings>");
            sb.AppendLine();
            sb.AppendLine(AddTabs(1) + "<connectionStrings>");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + connection_string.ToConfigString());
            sb.AppendLine();
            sb.AppendLine(AddTabs(1) + "</connectionStrings>");
            sb.AppendLine();
            sb.AppendLine(AddTabs(1) + "<system.web>");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "<compilation debug=\"true\" />");
            sb.AppendLine(AddTabs(2) + "<authentication mode=\"Windows\" />");
            sb.AppendLine();
            sb.AppendLine(AddTabs(1) + "</system.web>");
            sb.AppendLine();
            sb.AppendLine("</configuration>");

            output.Body = sb.ToString();
            return output;
        }

        public static OutputObject GenerateMasterPageCodeInFront(string database_name, List<string> selected_tables)
        {
            if (string.IsNullOrEmpty(database_name))
                return null;

            var output = new OutputObject();
            output.Name = ConfigurationManager.AppSettings["DefaultMasterPageFilename"];
            output.Type = OutputObject.eObjectType.CSharp;

            var sb = new StringBuilder();

            sb.AppendLine("<%@ Master Language=\"C#\" AutoEventWireup=\"true\" CodeFile=\"" + ConfigurationManager.AppSettings["DefaultMasterPageFilename"] + ".cs\" Inherits=\"WebControls." + NameFormatter.ToCSharpPropertyName(database_name) + ".MasterPage\" %>");
            sb.AppendLine();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine();

            sb.AppendLine("<html>");
            sb.AppendLine("<head runat=\"server\">");
            sb.AppendLine(AddTabs(1) + $"<title>{NameFormatter.ToFriendlyName(database_name)}</title>");
            sb.AppendLine(AddTabs(1) + "<link href=\"" + ConfigurationManager.AppSettings["DefaultCSSFilename"] + ".css\" rel=\"stylesheet\" type=\"text/css\" />");
            sb.AppendLine(AddTabs(1) + "<asp:contentplaceholder id=\"head\" runat=\"server\">");
            sb.AppendLine(AddTabs(1) + "</asp:contentplaceholder>");
            sb.AppendLine("</head>");
            sb.AppendLine();

            sb.AppendLine("<body>");
            sb.AppendLine(AddTabs(1) + "<form id=\"form1\" runat=\"server\">");
            sb.AppendLine();

            sb.AppendLine(AddTabs(1) + "<div id=\"header\">");
            sb.AppendLine(AddTabs(2) + "<div class=\"PageTitle\">Database " + NameFormatter.ToFriendlyName(database_name) + " Tools</div>");
            sb.AppendLine(AddTabs(2) + "<div>" + GenerateAuthorNotice() + "</div>");
            sb.AppendLine(AddTabs(1) + "</div>");
            sb.AppendLine();

            sb.AppendLine(AddTabs(1) + "<div id=\"menu\">");

            sb.AppendLine(AddTabs(1) + "<div class=\"SectionTitle\">Tables</div>");
            sb.AppendLine(AddTabs(1) + "<ul>");

            for (int i = 0; i < selected_tables.Count; i++)
            {
                // loop through table links here
                sb.AppendLine(AddTabs(2) + "<li><asp:HyperLink ID=\"HL" + (i + 1).ToString() + "\" NavigateUrl=\"List" + selected_tables[i] + ".aspx\" Text=\"" + selected_tables[i] + "\" runat=\"server\" /></li>");
            }

            sb.AppendLine(AddTabs(1) + "</ul>");
            sb.AppendLine(AddTabs(1) + "</div>");
            sb.AppendLine();
            sb.AppendLine(AddTabs(1) + "<div id=\"contents\">");
            sb.AppendLine(AddTabs(2) + "<asp:contentplaceholder id=\"body\" runat=\"server\">");
            sb.AppendLine(AddTabs(2) + "</asp:contentplaceholder>");
            sb.AppendLine(AddTabs(1) + "</div>");
            sb.AppendLine();

            sb.AppendLine(AddTabs(1) + "</form>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            output.Body = sb.ToString();
            return output;
        }
        public static OutputObject GenerateMasterPageCodeBehind(string database_name)
        {
            if (string.IsNullOrEmpty(database_name))
                return null;

            OutputObject output = new OutputObject();
            output.Name = ConfigurationManager.AppSettings["DefaultMasterPageFilename"] + ".cs";
            output.Type = OutputObject.eObjectType.CSharp;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("using System;");
            sb.AppendLine("using System.Data;");
            sb.AppendLine("using System.Configuration;");
            sb.AppendLine("using System.Collections;");
            sb.AppendLine("using System.Web;");
            sb.AppendLine("using System.Web.Security;");
            sb.AppendLine("using System.Web.UI;");
            sb.AppendLine("using System.Web.UI.WebControls;");
            sb.AppendLine("using System.Web.UI.WebControls.WebParts;");
            sb.AppendLine("using System.Web.UI.HtmlControls;");
            sb.AppendLine();

            sb.AppendLine("namespace WebControls." + NameFormatter.ToCSharpPropertyName(database_name) + "MasterPage");
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "public partial class MasterPage : System.Web.UI.MasterPage");
            sb.AppendLine(AddTabs(1) + "{");
            sb.AppendLine(AddTabs(2) + "protected void Page_Load(object sender, EventArgs e)");
            sb.AppendLine(AddTabs(2) + "{");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "}");
            sb.AppendLine(AddTabs(1) + "}");
            sb.AppendLine("}");

            output.Body = sb.ToString();
            return output;
        }

        public static OutputObject GenerateWebViewPageCodeInFront(SqlTable sqlTable)
        {
            if (sqlTable == null)
                return null;

            string view_class_name = "View" + NameFormatter.ToCSharpPropertyName(sqlTable.Name);
            string table_name = NameFormatter.ToCSharpPropertyName(sqlTable.Name);

            var output = new OutputObject();
            output.Name = view_class_name + ".aspx";
            output.Type = OutputObject.eObjectType.Aspx;

            var sb = new StringBuilder();

            sb.AppendLine($"<%@ Page Language=\"C#\" MasterPageFile=\"~/MasterPage.master\" AutoEventWireup=\"True\" CodeBehind=\"{view_class_name}.aspx.cs\" Inherits=\"WebControls.{NameFormatter.ToCSharpPropertyName(sqlTable.Database.Name)}.{view_class_name}\" %>");
            sb.AppendLine();
            sb.AppendLine("<asp:Content ID=\"Content1\" ContentPlaceHolderID=\"head\" Runat=\"Server\">");
            sb.AppendLine();

            /////////////////////////////////////////////////////////////////////////////////////////////

            sb.AppendLine(AddTabs(1) + "<script type = \"text/javascript\">");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "$(function()");
            sb.AppendLine(AddTabs(2) + "{");
            sb.AppendLine(AddTabs(3) + $"Load{table_name}();");
            sb.AppendLine(AddTabs(2) + "});");
            sb.AppendLine();

            sb.AppendLine(AddTabs(2) + $"function Load{table_name}()");
            sb.AppendLine(AddTabs(2) + "{");
            sb.AppendLine(AddTabs(3) + "var data = { };");
            sb.AppendLine(AddTabs(3) + $"CallAjax(data, '{output.Name}/Load{table_name}', Load{table_name}Response);");
            sb.AppendLine(AddTabs(2) + "}");

            sb.AppendLine(AddTabs(2) + $"function Load{table_name}Response(response)");
            sb.AppendLine(AddTabs(2) + "{");
            sb.AppendLine(AddTabs(3) + "if (response.Success)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "var buffer = '';");
            sb.AppendLine(AddTabs(4) + $"var template = $('#{table_name}Template').html();");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + $"response.DataList.forEach(function(i)");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "buffer += eval('`' + template + '`');");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + $"$('#{table_name}List').html(buffer);");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine(AddTabs(3) + "else");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "$('#txtMessage').text(response.Message);");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine();

            sb.AppendLine(AddTabs(2) + $"function Save{table_name}()");
            sb.AppendLine(AddTabs(2) + "{");
            sb.AppendLine(AddTabs(3) + "var data = { };");

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                string colum_name = NameFormatter.ToCSharpPropertyName(sql_column.Name);
                sb.AppendLine(AddTabs(3) + $"data.{colum_name} = $('#txt{colum_name}').val();");
            }

            sb.AppendLine();
            sb.AppendLine(AddTabs(3) + $"CallAjax(data, '{output.Name}/Save{table_name}', Save{table_name}Response);");
            sb.AppendLine(AddTabs(2) + "}");

            sb.AppendLine(AddTabs(2) + $"function Save{table_name}Response(response)");
            sb.AppendLine(AddTabs(2) + "{");
            sb.AppendLine(AddTabs(3) + "if (response.Success)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + $"Load{table_name}();");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine(AddTabs(3) + "else");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "$('#txtMessage').text(response.Message);");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine(AddTabs(2) + "}");
            sb.AppendLine();

            sb.AppendLine(AddTabs(2) + "</script>");
            sb.AppendLine();

            sb.AppendLine(AddTabs(2) + $"<script type = 'text/template' id='{table_name}Template'>");
            sb.AppendLine(AddTabs(2) + "<tr>");

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                string colum_name = NameFormatter.ToCSharpPropertyName(sql_column.Name);
                sb.AppendLine(AddTabs(3) + $"<td>${{i.{colum_name}}}</td>");
            }

            sb.AppendLine(AddTabs(2) + "</tr>");
            sb.AppendLine(AddTabs(2) + "</script>");
            sb.AppendLine();
            sb.AppendLine("</asp:Content>");
            sb.AppendLine();

            /////////////////////////////////////////////////////////////////////////////////////////////

            sb.AppendLine("<asp:Content ID=\"Content2\" ContentPlaceHolderID=\"body\" Runat=\"Server\">");
            sb.AppendLine();

            sb.AppendLine(AddTabs(1) + $"<div id='pan{table_name}List'>");
            sb.AppendLine(AddTabs(2) + $"<div style='overflow: auto;'>");

            //< div class="SectionTitle" style="float: left;">Languages</div>
            //<div style = "float: right;" >
            //< input type="button" class="Button" value="Add New Language" onclick="$('#dialog-AddLanguage').dialog('open');" />
            //</div>
            //</div>

            //<table>
            //<thead>
            //<tr>
            //<th>Language</th>
            //<th>Family</th>
            //<th>Format</th>
            //<th>Difficulty</th>
            //<th>Rarity</th>
            //</tr>
            //</thead>
            //<tbody id = "LangaugeList" ></ tbody >
            //</ table >
            //</ div >








            sb.AppendLine();
            sb.AppendLine("<div class=\"PageTitle\">View " + NameFormatter.ToFriendlyName(sqlTable.Name) + "</div>");
            sb.AppendLine();

            sb.AppendLine("<asp:Panel CssClass=\"DetailsPanel\" ID=\"panDetails\" runat=\"server\">");
            sb.AppendLine();
            sb.AppendLine(AddTabs(1) + "<div class=\"Spacer\">");
            sb.AppendLine();

            sb.AppendLine(AddTabs(2) + "<table border=\"0\" width=\"100%\">");
            sb.AppendLine(AddTabs(2) + "<tr>");
            sb.AppendLine(AddTabs(3) + "<td></td>");
            sb.AppendLine(AddTabs(3) + "<td class=\"SectionTitle\"><asp:Label ID=\"lblTitle\" runat=\"server\" /></td>");
            sb.AppendLine(AddTabs(2) + "</tr>");
            sb.AppendLine();

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if (!sql_column.IsPk)
                {
                    sb.AppendLine(AddTabs(2) + "<tr>");
                    sb.AppendLine(AddTabs(3) + "<td class=\"ControlLabel\">" + NameFormatter.ToFriendlyName(sql_column.Name) + ":</td>");

                    switch (sql_column.BaseType)
                    {
                        case eSqlBaseType.Bool:
                            sb.AppendLine(AddTabs(3) + "<td><asp:CheckBox ID=\"chk" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + "\" Enabled=\"False\"  runat=\"server\" /></td>");
                            break;

                        //case eSqlBaseType.Time:
                        //sb.AppendLine(AddTabs(3) + "<td><asp:Calendar ID=\"cal" + NameFormatter.ToCSharpPropertyName(sql_column.ColumnName) + " runat=\"server\" /></td>");
                        //break;

                        default:
                            sb.AppendLine(AddTabs(3) + "<td><asp:Label ID=\"lbl" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + "\" runat=\"server\" /></td>");
                            break;
                    }

                    sb.AppendLine(AddTabs(2) + "</tr>");
                    sb.AppendLine();
                }
            }

            // buttons
            sb.AppendLine(AddTabs(2) + "</table>");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "<asp:Label ID=\"lblErrorMessage\" CssClass=\"ErrorMessage\" runat=\"server\" />");
            sb.AppendLine();
            sb.AppendLine(AddTabs(1) + "</div>");
            sb.AppendLine();
            sb.AppendLine("</asp:Panel>");

            output.Body = sb.ToString();
            return output;
        }
        public static OutputObject GenerateWebViewPageCodeBehind(SqlTable sql_table, List<string> namespace_includes)
        {
            if (sql_table == null)
                return null;

            string view_class_name = "View" + NameFormatter.ToCSharpPropertyName(sql_table.Name);
            int longest_column = GetLongestColumnLength(sql_table) + sql_table.Name.Length;

            OutputObject output = new OutputObject();
            output.Name = view_class_name + ".aspx.cs";
            output.Type = OutputObject.eObjectType.CSharp;

            namespace_includes.Add(NameFormatter.ToCSharpPropertyName(sql_table.Database.Name));

            StringBuilder sb = new StringBuilder();

            #region Includes
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Data;");
            sb.AppendLine("using System.Configuration;");
            sb.AppendLine("using System.Collections;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Web;");
            sb.AppendLine("using System.Web.Security;");
            sb.AppendLine("using System.Web.UI;");
            sb.AppendLine("using System.Web.UI.WebControls;");
            sb.AppendLine("using System.Web.UI.WebControls.WebParts;");
            sb.AppendLine("using System.Web.UI.HtmlControls;");
            sb.AppendLine();

            sb.AppendLine(GenerateNamespaceIncludes(namespace_includes));
            sb.AppendLine();

            #endregion

            sb.AppendLine("namespace WebControls." + NameFormatter.ToCSharpPropertyName(sql_table.Database.Name));
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "public partial class " + view_class_name + " : System.Web.UI.Page");
            sb.AppendLine(AddTabs(1) + "{");

            sb.AppendLine(AddTabs(2) + "#region Fields");
            sb.AppendLine();

            #region Fields
            sb.AppendLine(AddTabs(3) + "protected string _SQLConnection = ConfigurationManager.ConnectionStrings[\"SQLConnection\"].ConnectionString;");
            #endregion

            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#endregion");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#region Properties");
            sb.AppendLine();

            #region Properties
            sb.AppendLine(AddTabs(3) + "protected string PageName");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "get { return \"" + view_class_name + "\"; }");
            sb.AppendLine(AddTabs(3) + "}");

            sb.AppendLine(AddTabs(3) + "protected PaginationManager PageinationData");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "get");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "if (ViewState[\"PaginationManager\"] == null)");
            sb.AppendLine(AddTabs(6) + "ViewState[\"PaginationManager\"] = new PaginationManager();");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "return (PaginationManager)ViewState[\"PaginationManager\"];");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(4) + "set");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "ViewState[\"PaginationManager\"] = value;");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(3) + "}");

            sb.AppendLine(AddTabs(3) + "protected string ReturnPage");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "get");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "if (ViewState[\"ReturnPage\"] == null)");
            sb.AppendLine(AddTabs(6) + "ViewState[\"ReturnPage\"] = \"~/Default.aspx\";");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "return (string)ViewState[\"ReturnPage\"];");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(4) + "set");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "ViewState[\"ReturnPage\"] = value;");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(3) + "}");

            sb.AppendLine(AddTabs(3) + "protected int EditingID");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "get");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "if (ViewState[\"EditingID\"] == null)");
            sb.AppendLine(AddTabs(6) + "ViewState[\"EditingID\"] = 0;");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "return (int)ViewState[\"EditingID\"];");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(4) + "set");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "ViewState[\"EditingID\"] = value;");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#endregion");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#region Methods");
            sb.AppendLine();

            #region BindForm Method
            sb.AppendLine(AddTabs(3) + "protected void BindForm()");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + NameFormatter.ToCSharpClassName("DAL" + sql_table.Name) + " dal_obj = new " + NameFormatter.ToCSharpClassName("DAL" + sql_table.Name) + "(_SQLConnection);");
            sb.AppendLine(AddTabs(4) + "dal_obj.LoadSingleFromDb(this.EditingID);");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "if (dal_obj.Collection.Count > 0)");
            sb.AppendLine(AddTabs(4) + "{");

            foreach (var sql_column in sql_table.Columns.Values)
            {
                if (!sql_column.IsPk)
                {
                    // need to account for datatypes that need to be converted to a string.

                    switch (sql_column.BaseType)
                    {
                        case eSqlBaseType.Bool:
                            sb.AppendLine(AddTabs(5) + PadCSharpVariableName("chk" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Checked", longest_column, 11) + "= dal_obj.Collection[0]." + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ";");
                            break;

                        case eSqlBaseType.Guid:
                            sb.AppendLine(AddTabs(5) + PadCSharpVariableName("lbl" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Text", longest_column, 11) + "= dal_obj.Collection[0]." + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".ToString();");
                            break;

                        case eSqlBaseType.Integer:
                            sb.AppendLine(AddTabs(5) + PadCSharpVariableName("lbl" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Text", longest_column, 11) + "= dal_obj.Collection[0]." + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".ToString();");
                            break;

                        case eSqlBaseType.Float:
                            sb.AppendLine(AddTabs(5) + PadCSharpVariableName("lbl" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Text", longest_column, 11) + "= dal_obj.Collection[0]." + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".ToString();");
                            break;

                        case eSqlBaseType.Time:
                            sb.AppendLine(AddTabs(5) + PadCSharpVariableName("lbl" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Text", longest_column, 11) + "= dal_obj.Collection[0]." + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".ToString();");
                            break;

                        default:
                            sb.AppendLine(AddTabs(5) + PadCSharpVariableName("lbl" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Text", longest_column, 11) + "= dal_obj.Collection[0]." + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ";");
                            break;
                    }
                }
            }
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(4) + "else");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "this.lblErrorMessage.Text = \"Load Failed\";");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            #region Check Credentials method
            sb.AppendLine(AddTabs(3) + "protected void CheckCredentials()");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#endregion");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#region Events");
            sb.AppendLine();

            #region pageload event method
            sb.AppendLine(AddTabs(3) + "protected void Page_Load(object sender, EventArgs e)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "CheckCredentials();");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "if (!Page.IsPostBack)");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "if (Request.QueryString[\"Id\"] != null)");
            sb.AppendLine(AddTabs(6) + "this.EditingID = Convert.ToInt32(Request.QueryString[\"Id\"]);");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "if (Request.QueryString[\"ReturnPage\"] != null)");
            sb.AppendLine(AddTabs(6) + "this.ReturnPage = (string)Request.QueryString[\"ReturnPage\"];");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "if (Request.QueryString[\"PageData\"] != null)");
            sb.AppendLine(AddTabs(6) + "this.PageinationData = new PaginationManager(Request.QueryString[\"PageData\"].ToString());");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "BindForm();");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#endregion");

            sb.AppendLine(AddTabs(1) + "}");
            sb.AppendLine("}");

            output.Body = sb.ToString();
            return output;
        }

        public static OutputObject GenerateWebEditPageCodeInFront(SqlTable sql_table, bool create_as_asp_control)
        {
            if (sql_table == null)
                return null;

            string list_class_name = "List" + NameFormatter.ToCSharpPropertyName(sql_table.Name);
            string edit_class_name = "Edit" + NameFormatter.ToCSharpPropertyName(sql_table.Name);

            OutputObject output = new OutputObject();
            output.Name = edit_class_name + ".aspx";
            output.Type = OutputObject.eObjectType.Aspx;

            StringBuilder sb = new StringBuilder();

            if (create_as_asp_control)
            {
                sb.AppendLine("<%@ Control Language=\"C#\" AutoEventWireup=\"true\" CodeBehind=\"" + edit_class_name + ".aspx.cs\" Inherits=\"WebControls." + NameFormatter.ToCSharpPropertyName(sql_table.Database.Name) + "." + edit_class_name + "\" %>");
            }
            else
            {
                sb.AppendLine("<%@ Page Language=\"C#\" MasterPageFile=\"~/MasterPage.master\" AutoEventWireup=\"True\" CodeBehind=\"" + edit_class_name + ".aspx.cs\" Inherits=\"WebControls." + NameFormatter.ToCSharpPropertyName(sql_table.Database.Name) + "." + edit_class_name + "\" %>");
                sb.AppendLine();
                sb.AppendLine("<asp:Content ID=\"Content1\" ContentPlaceHolderID=\"head\" Runat=\"Server\">");
                sb.AppendLine("</asp:Content>");
                sb.AppendLine();
                sb.AppendLine("<asp:Content ID=\"Content2\" ContentPlaceHolderID=\"body\" Runat=\"Server\">");
            }

            sb.AppendLine();
            sb.AppendLine("<div class=\"PageTitle\">" + NameFormatter.ToFriendlyName(sql_table.Name) + "</div>");
            sb.AppendLine();

            sb.AppendLine("<asp:Panel CssClass=\"DetailsPanel\" ID=\"panDetails\" runat=\"server\">");
            sb.AppendLine();
            sb.AppendLine(AddTabs(1) + "<div class=\"Spacer\">");
            sb.AppendLine();

            sb.AppendLine(AddTabs(2) + "<table border=\"0\" width=\"100%\">");
            sb.AppendLine(AddTabs(2) + "<tr>");
            sb.AppendLine(AddTabs(3) + "<td></td>");
            sb.AppendLine(AddTabs(3) + "<td class=\"SectionTitle\"><asp:Label ID=\"lblTitle\" runat=\"server\" /></td>");
            sb.AppendLine(AddTabs(2) + "</tr>");
            sb.AppendLine();

            foreach (var sql_column in sql_table.Columns.Values)
            {
                if (!sql_column.IsPk)
                {
                    sb.AppendLine(AddTabs(2) + "<tr>");
                    sb.AppendLine(AddTabs(3) + "<td class=\"ControlLabel\">" + NameFormatter.ToFriendlyName(sql_column.Name) + ":</td>");

                    switch (sql_column.BaseType)
                    {
                        case eSqlBaseType.Bool:
                            sb.AppendLine(AddTabs(3) + "<td><asp:CheckBox ID=\"chk" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + "\" runat=\"server\" /></td>");
                            break;

                        case eSqlBaseType.Time:
                            sb.AppendLine(AddTabs(3) + "<td><asp:TextBox ID=\"txt" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + "\" runat=\"server\" /></td>");
                            break;

                        default:
                            if (sql_column.BaseType == eSqlBaseType.String)
                            {
                                if (sql_column.Length == -1)
                                {
                                    sb.AppendLine(AddTabs(3) + "<td><asp:TextBox ID=\"txt" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + "\" runat=\"server\" TextMode=\"MultiLine\" Rows=\"8\" Width=\"400\" /></td>");
                                }
                                else if (sql_column.Length > 100)
                                {
                                    sb.AppendLine(AddTabs(3) + "<td><asp:TextBox ID=\"txt" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + "\" runat=\"server\" Width=\"400\" Rows=\"" + (sql_column.Length / 100).ToString() + "\" TextMode=\"MultiLine\" /></td>");
                                }
                                else
                                {
                                    sb.AppendLine(AddTabs(3) + "<td><asp:TextBox ID=\"txt" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + "\" runat=\"server\" Width=\"400\" MaxLength=\"" + sql_column.Length.ToString() + "\" /></td>");
                                }
                            }
                            else
                            {
                                //This might need to have the max length fixed. ints and stuff can end up here.
                                sb.AppendLine(AddTabs(3) + "<td><asp:TextBox ID=\"txt" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + "\" runat=\"server\" Width=\"400\" MaxLength=\"" + sql_column.Length.ToString() + "\" /></td>");
                            }
                            break;
                    }

                    sb.AppendLine(AddTabs(2) + "</tr>");
                    sb.AppendLine();
                }
            }

            // buttons
            sb.AppendLine(AddTabs(2) + "<tr>");
            sb.AppendLine(AddTabs(3) + "<td></td>");
            sb.AppendLine(AddTabs(3) + "<td class=\"SectionTitle\">");
            sb.AppendLine(AddTabs(4) + "<asp:Button ID=\"btnSave\" runat=\"server\" CssClass=\"Button1\" Text=\"Save\" OnClick=\"btnSave_Click\" />");
            sb.AppendLine(AddTabs(4) + "<asp:Button ID=\"btnUndo\" runat=\"server\" CssClass=\"Button1\" Text=\"Undo Changes\" OnClick=\"btnUndo_Click\" />");
            sb.AppendLine(AddTabs(3) + "</td>");
            sb.AppendLine(AddTabs(2) + "</tr>");
            sb.AppendLine(AddTabs(2) + "</table>");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "<asp:Label ID=\"lblErrorMessage\" CssClass=\"ErrorMessage\" runat=\"server\" />");
            sb.AppendLine();
            sb.AppendLine(AddTabs(1) + "</div>");
            sb.AppendLine();
            sb.AppendLine("</asp:Panel>");

            if (!create_as_asp_control)
            {
                sb.AppendLine();
                sb.AppendLine("</asp:Content>");
            }

            output.Body = sb.ToString();
            return output;
        }
        public static OutputObject GenerateWebEditPageCodeBehind(SqlTable sql_table, List<string> namespace_includes)
        {
            if (sql_table == null)
                return null;

            string class_name = "Edit" + NameFormatter.ToCSharpPropertyName(sql_table.Name);
            int longest_column = GetLongestColumnLength(sql_table) + sql_table.Name.Length;

            OutputObject output = new OutputObject();
            output.Name = class_name + ".aspx.cs";
            output.Type = OutputObject.eObjectType.CSharp;

            namespace_includes.Add(NameFormatter.ToCSharpPropertyName(sql_table.Database.Name));

            StringBuilder sb = new StringBuilder();

            #region Includes
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Data;");
            sb.AppendLine("using System.Configuration;");
            sb.AppendLine("using System.Collections;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Web;");
            sb.AppendLine("using System.Web.Security;");
            sb.AppendLine("using System.Web.UI;");
            sb.AppendLine("using System.Web.UI.WebControls;");
            sb.AppendLine("using System.Web.UI.WebControls.WebParts;");
            sb.AppendLine("using System.Web.UI.HtmlControls;");
            sb.AppendLine();

            sb.AppendLine(GenerateNamespaceIncludes(namespace_includes));
            sb.AppendLine();

            #endregion

            sb.AppendLine("namespace WebControls." + NameFormatter.ToCSharpPropertyName(sql_table.Database.Name));
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "public partial class " + class_name + " : System.Web.UI.Page");
            sb.AppendLine(AddTabs(1) + "{");

            #region Fields
            sb.AppendLine(AddTabs(2) + "#region Fields");
            sb.AppendLine();
            sb.AppendLine(AddTabs(3) + "protected string _SQLConnection = ConfigurationManager.ConnectionStrings[\"SQLConnection\"].ConnectionString;");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#endregion");
            sb.AppendLine();
            #endregion

            #region Properties
            sb.AppendLine(AddTabs(2) + "#region Properties");
            sb.AppendLine();

            sb.AppendLine(AddTabs(3) + "protected string PageName");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "get { return \"" + class_name + "\"; }");
            sb.AppendLine(AddTabs(3) + "}");

            sb.AppendLine(AddTabs(3) + "protected PaginationManager PageinationData");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "get");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "if (ViewState[\"PaginationManager\"] == null)");
            sb.AppendLine(AddTabs(6) + "ViewState[\"PaginationManager\"] = new PaginationManager();");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "return (PaginationManager)ViewState[\"PaginationManager\"];");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(4) + "set");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "ViewState[\"PaginationManager\"] = value;");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(3) + "}");

            sb.AppendLine(AddTabs(3) + "protected string ReturnPage");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "get");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "if (ViewState[\"ReturnPage\"] == null)");
            sb.AppendLine(AddTabs(6) + "ViewState[\"ReturnPage\"] = \"~/Default.aspx\";");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "return (string)ViewState[\"ReturnPage\"];");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(4) + "set");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "ViewState[\"ReturnPage\"] = value;");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(3) + "}");

            sb.AppendLine(AddTabs(3) + "protected int EditingID");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "get");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "if (ViewState[\"EditingID\"] == null)");
            sb.AppendLine(AddTabs(6) + "ViewState[\"EditingID\"] = 0;");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "return (int)ViewState[\"EditingID\"];");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(4) + "set");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "ViewState[\"EditingID\"] = value;");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(3) + "}");

            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#endregion");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#region Methods");
            sb.AppendLine();
            #endregion

            #region BindForm Method
            sb.AppendLine(AddTabs(3) + "protected void BindForm()");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + NameFormatter.ToCSharpClassName("DAL" + sql_table.Name) + " dal_obj = new " + NameFormatter.ToCSharpClassName("DAL" + sql_table.Name) + "(_SQLConnection);");
            sb.AppendLine(AddTabs(4) + "dal_obj.LoadSingleFromDb(this.EditingID);");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "if (dal_obj.Collection.Count > 0)");
            sb.AppendLine(AddTabs(4) + "{");

            foreach (var sql_column in sql_table.Columns.Values)
            {
                if (!sql_column.IsPk)
                {
                    // need to account for datatypes that need to be converted to a string.

                    switch (sql_column.BaseType)
                    {
                        case eSqlBaseType.Bool:
                            sb.AppendLine(AddTabs(5) + PadCSharpVariableName("chk" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Checked", longest_column, 11) + "= dal_obj.Collection[0]." + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ";");
                            break;

                        case eSqlBaseType.Guid:
                            sb.AppendLine(AddTabs(5) + PadCSharpVariableName("txt" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Text", longest_column, 11) + "= dal_obj.Collection[0]." + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".ToString();");
                            break;

                        case eSqlBaseType.Integer:
                            sb.AppendLine(AddTabs(5) + PadCSharpVariableName("txt" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Text", longest_column, 11) + "= dal_obj.Collection[0]." + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".ToString();");
                            break;

                        case eSqlBaseType.Float:
                            sb.AppendLine(AddTabs(5) + PadCSharpVariableName("txt" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Text", longest_column, 11) + "= dal_obj.Collection[0]." + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".ToString();");
                            break;

                        case eSqlBaseType.Time:
                            sb.AppendLine(AddTabs(5) + PadCSharpVariableName("txt" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Text", longest_column, 11) + "= dal_obj.Collection[0]." + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".ToString();");
                            break;

                        default:
                            sb.AppendLine(AddTabs(5) + PadCSharpVariableName("txt" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Text", longest_column, 11) + "= dal_obj.Collection[0]." + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ";");
                            break;
                    }
                }
            }
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(4) + "else");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "this.lblErrorMessage.Text = \"Load Failed\";");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            #region Check Credentials method
            sb.AppendLine(AddTabs(3) + "protected void CheckCredentials()");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            #region Clear form method
            sb.AppendLine(AddTabs(3) + "protected void ClearForm()");
            sb.AppendLine(AddTabs(3) + "{");

            foreach (var sql_column in sql_table.Columns.Values)
            {
                if (!sql_column.IsPk)
                {
                    switch (sql_column.BaseType)
                    {
                        case eSqlBaseType.Bool:
                            sb.AppendLine(AddTabs(4) + PadCSharpVariableName("chk" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Checked", longest_column, 11) + "= false;");
                            break;

                        case eSqlBaseType.Time:
                            sb.AppendLine(AddTabs(4) + PadCSharpVariableName("txt" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Text", longest_column, 11) + "= string.Empty;");
                            break;

                        default:
                            sb.AppendLine(AddTabs(4) + PadCSharpVariableName("txt" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Text", longest_column, 11) + "= string.Empty;");
                            break;
                    }
                }
            }

            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "lblErrorMessage.Text = string.Empty;");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            #region UpdateDb Method
            sb.AppendLine(AddTabs(3) + "protected bool UpdateDb()");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "try");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + NameFormatter.ToCSharpClassName("DAL" + sql_table.Name) + " dal_obj = new " + NameFormatter.ToCSharpClassName("DAL" + sql_table.Name) + "(_SQLConnection);");
            sb.AppendLine(AddTabs(5) + NameFormatter.ToCSharpClassName(sql_table.Name) + " obj = new " + NameFormatter.ToCSharpClassName(sql_table.Name) + "();");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "if (this.EditingID > 0)");
            sb.AppendLine(AddTabs(5) + "{");
            sb.AppendLine(AddTabs(6) + "dal_obj.LoadSingleFromDb(this.EditingID);");
            sb.AppendLine();
            sb.AppendLine(AddTabs(6) + "if (dal_obj.Collection.Count > 0)");
            sb.AppendLine(AddTabs(7) + "obj = dal_obj.Collection[0];");
            sb.AppendLine(AddTabs(5) + "}");
            sb.AppendLine();

            foreach (var sql_column in sql_table.Columns.Values)
            {
                if (!sql_column.IsPk)
                {
                    string csharp_columnname = NameFormatter.ToCSharpPropertyName(sql_column.Name);

                    sb.Append(AddTabs(5) + PadCSharpVariableName("obj." + NameFormatter.ToCSharpPropertyName(sql_column.Name), longest_column, 4));

                    switch (sql_column.BaseType)
                    {
                        case eSqlBaseType.Bool:
                            sb.AppendLine("= chk" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Checked;");
                            break;

                        case eSqlBaseType.Time:
                            sb.AppendLine("= Convert.ToDateTime(txt" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Text);");
                            break;

                        case eSqlBaseType.Integer:
                            sb.AppendLine("= Convert.ToInt32(txt" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Text);");
                            break;

                        case eSqlBaseType.Float:
                            sb.AppendLine("= Convert.ToDouble(txt" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Text);");
                            break;

                        case eSqlBaseType.Guid:
                            sb.AppendLine("= new Guid(txt" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Text);");
                            break;

                        default:
                            sb.AppendLine("= txt" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + ".Text;");
                            break;
                    }
                }
            }

            sb.AppendLine();

            sb.AppendLine(AddTabs(5) + "dal_obj.Save(obj);");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(4) + "catch");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "return false;");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "return true;");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            #region Validation method
            sb.AppendLine(AddTabs(3) + "protected bool ValidateForm()");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "return true;");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#endregion");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#region Events");
            sb.AppendLine();

            #region pageload event method
            sb.AppendLine(AddTabs(3) + "protected void Page_Load(object sender, EventArgs e)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "CheckCredentials();");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "this.btnSave.Focus();");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "if (!Page.IsPostBack)");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "if (Request.QueryString[\"Id\"] != null)");
            sb.AppendLine(AddTabs(6) + "this.EditingID = Convert.ToInt32(Request.QueryString[\"Id\"]);");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "if (Request.QueryString[\"ReturnPage\"] != null)");
            sb.AppendLine(AddTabs(6) + "this.ReturnPage = (string)Request.QueryString[\"ReturnPage\"];");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "if (Request.QueryString[\"PageData\"] != null)");
            sb.AppendLine(AddTabs(6) + "this.PageinationData = new PaginationManager(Request.QueryString[\"PageData\"].ToString());");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "if (this.EditingID == 0)");
            sb.AppendLine(AddTabs(5) + "{");
            sb.AppendLine(AddTabs(6) + "this.lblTitle.Text = \"Add New Item\";");
            sb.AppendLine(AddTabs(6) + "ClearForm();");
            sb.AppendLine(AddTabs(5) + "}");
            sb.AppendLine(AddTabs(5) + "else");
            sb.AppendLine(AddTabs(5) + "{");
            sb.AppendLine(AddTabs(6) + "this.lblTitle.Text = \"Edit Item\";");
            sb.AppendLine(AddTabs(6) + "BindForm();");
            sb.AppendLine(AddTabs(5) + "}");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            #region btnSave_Click event
            sb.AppendLine(AddTabs(3) + "protected void btnSave_Click(object sender, EventArgs e)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "bool results = ValidateForm();");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "if (results)");
            sb.AppendLine(AddTabs(5) + "results = UpdateDb();");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "if (results)");
            sb.AppendLine(AddTabs(5) + "Response.Redirect(this.ReturnPage + \".aspx?PageData=\" + this.PageinationData.ToString(), true);");
            sb.AppendLine(AddTabs(4) + "else");
            sb.AppendLine(AddTabs(5) + "lblErrorMessage.Text = \"Save failed\";");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            #region Undo button
            sb.AppendLine(AddTabs(3) + "protected void btnUndo_Click(object sender, EventArgs e)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "if (this.EditingID > 0)");
            sb.AppendLine(AddTabs(5) + "BindForm();");
            sb.AppendLine(AddTabs(4) + "else");
            sb.AppendLine(AddTabs(5) + "ClearForm();");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#endregion");
            sb.AppendLine(AddTabs(1) + "}");
            sb.AppendLine("}");

            output.Body = sb.ToString();
            return output;
        }

        public static OutputObject GenerateWebListPageCodeInFront(SqlTable sql_table, bool create_as_asp_control)
        {
            if (sql_table == null)
                return null;

            string list_object_name = "List" + NameFormatter.ToCSharpPropertyName(sql_table.Name);
            string edit_object_name = "Edit" + NameFormatter.ToCSharpPropertyName(sql_table.Name);
            string orm_class_name = NameFormatter.ToCSharpClassName(sql_table.Name);
            string list_class_name = NameFormatter.ToCSharpClassName(list_object_name);

            OutputObject output = new OutputObject();
            output.Name = list_object_name + ".aspx";
            output.Type = OutputObject.eObjectType.Aspx;

            bool first_flag = true;

            StringBuilder sb = new StringBuilder();

            if (create_as_asp_control)
            {
                sb.AppendLine("<%@ Control Language=\"C#\" AutoEventWireup=\"true\" CodeBehind=\"" + list_object_name + ".aspx.cs\" Inherits=\"WebControls." + NameFormatter.ToCSharpPropertyName(sql_table.Database.Name) + "." + list_object_name + "\" %>");
            }
            else
            {
                sb.AppendLine("<%@ Page Language=\"C#\" MasterPageFile=\"~/MasterPage.master\" AutoEventWireup=\"true\" CodeBehind=\"" + list_object_name + ".aspx.cs\" Inherits=\"WebControls." + NameFormatter.ToCSharpPropertyName(sql_table.Database.Name) + "." + list_object_name + "\" %>");
                sb.AppendLine();
                sb.AppendLine("<asp:Content ID=\"Content1\" ContentPlaceHolderID=\"head\" Runat=\"Server\">");
                sb.AppendLine("</asp:Content>");
                sb.AppendLine();
                sb.AppendLine("<asp:Content ID=\"Content2\" ContentPlaceHolderID=\"body\" Runat=\"Server\">");
            }

            sb.AppendLine("<asp:Panel ID=\"panList\" DefaultButton=\"btnSearch\" runat=\"server\">");
            sb.AppendLine();

            sb.AppendLine(AddTabs(1) + "<div class=\"PageTitle\">" + NameFormatter.ToFriendlyName(sql_table.Name));
            sb.AppendLine(AddTabs(1) + "<asp:LinkButton ID=\"btnAddNewItem\" OnClick=\"btnAddNewItem_Click\" runat=\"server\" Text=\"(Add new item)\" CssClass=\"AddItemLink\" />");
            sb.AppendLine(AddTabs(1) + "</div>");
            sb.AppendLine();

            sb.AppendLine(AddTabs(1) + "<asp:TextBox ID=\"txtSearch\" runat=\"server\" />");
            sb.AppendLine(AddTabs(1) + "<asp:Button ID=\"btnSearch\" runat=\"server\" CssClass=\"Button1\" Text=\"Search\" OnClick=\"btnSearch_Click\" />");
            sb.AppendLine();

            #region Repeater
            sb.AppendLine(AddTabs(1) + "<asp:Repeater ID=\"rptList\" runat=\"server\" >");
            sb.AppendLine();

            sb.AppendLine(AddTabs(2) + "<HeaderTemplate>");
            sb.AppendLine(AddTabs(2) + "<table width=\"100%\">");
            sb.AppendLine(AddTabs(2) + "<tr class=\"RepeaterHeaderStyle\">");
            sb.AppendLine();

            foreach (var sql_column in sql_table.Columns.Values)
            {
                sb.AppendLine(AddTabs(3) + "<td><span class=\"RepeaterHeader\">" + NameFormatter.ToFriendlyName(sql_column.Name) + "</span></td>");
            }

            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "</tr>");
            sb.AppendLine(AddTabs(2) + "</HeaderTemplate>");
            sb.AppendLine();

            // Item template
            sb.AppendLine(AddTabs(2) + "<ItemTemplate>");
            sb.AppendLine(AddTabs(2) + "<tr class=\"RepeaterItemStyle\">");
            sb.AppendLine();

            first_flag = true;

            foreach (var item in sql_table.Columns.Values)
            {
                string current_item_binding = "((" + sql_table.Database.Name + "." + orm_class_name + ")Container.DataItem)." + NameFormatter.ToCSharpPropertyNameString(item);

                if (first_flag)
                {
                    first_flag = false;
                    sb.AppendLine(AddTabs(3) + "<td valign=\"top\"><a href=\"" + edit_object_name + ".aspx?id=<%# " + current_item_binding + " %>&ReturnPage=" + list_object_name + "&PageData=<%=PageinationData.ToString()%>\"><%# " + current_item_binding + " %></a></td>");
                }
                else
                {
                    sb.AppendLine(AddTabs(3) + "<td valign=\"top\"><span class=\"RepeaterCell\"><%# " + current_item_binding + " %></span></td>");
                }
            }

            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "</tr>");
            sb.AppendLine(AddTabs(2) + "</ItemTemplate>");
            sb.AppendLine();

            // Alternating item template
            sb.AppendLine(AddTabs(2) + "<AlternatingItemTemplate>");
            sb.AppendLine(AddTabs(2) + "<tr class=\"AlternatingRepeaterItemStyle\">");
            sb.AppendLine();

            first_flag = true;

            foreach (var item in sql_table.Columns.Values)
            {
                string current_item_binding = "((" + sql_table.Database.Name + "." + orm_class_name + ")Container.DataItem)." + NameFormatter.ToCSharpPropertyNameString(item);

                if (first_flag)
                {
                    first_flag = false;
                    sb.AppendLine(AddTabs(3) + "<td valign=\"top\"><a href=\"" + edit_object_name + ".aspx?id=<%# " + current_item_binding + " %>&ReturnPage=" + list_object_name + "&PageData=<%=PageinationData.ToString()%>\"><%# " + current_item_binding + " %></a></td>");
                }
                else
                {
                    sb.AppendLine(AddTabs(3) + "<td valign=\"top\"><span class=\"RepeaterCell\"><%# " + current_item_binding + " %></span></td>");
                }
            }

            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "</tr>");
            sb.AppendLine(AddTabs(2) + "</AlternatingItemTemplate>");
            sb.AppendLine();

            sb.AppendLine(AddTabs(2) + "<FooterTemplate>");
            sb.AppendLine(AddTabs(2) + "</table>");
            sb.AppendLine(AddTabs(2) + "</FooterTemplate>");
            sb.AppendLine();
            sb.AppendLine(AddTabs(1) + "</asp:Repeater>");
            #endregion

            sb.AppendLine();
            sb.AppendLine(AddTabs(1) + "<asp:Button ID=\"btnFirstPage\" runat=\"server\" CssClass=\"Button1\" OnClick=\"btnFirstPage_Click\" Text=\"&lt;&lt;\" />");
            sb.AppendLine(AddTabs(1) + "<asp:Button ID=\"btnPrevPage\" runat=\"server\" CssClass=\"Button1\" OnClick=\"btnPrevPage_Click\" Text=\"&lt;\" />");
            sb.AppendLine(AddTabs(1) + "<asp:Label ID=\"lblCounter\" runat=\"server\" Text=\"Label\" />");
            sb.AppendLine(AddTabs(1) + "<asp:Button ID=\"btnNextPage\" runat=\"server\" CssClass=\"Button1\" OnClick=\"btnNextPage_Click\" Text=\"&gt;\" />");
            sb.AppendLine(AddTabs(1) + "<asp:Button ID=\"btnLastPage\" runat=\"server\" CssClass=\"Button1\" OnClick=\"btnLastPage_Click\" Text=\"&gt;&gt;\" />");
            sb.AppendLine(AddTabs(1) + "<asp:DropDownList ID=\"ddlPageSize\" runat=\"server\" OnSelectedIndexChanged=\"ddlPageSize_SelectedIndexChanged\" AutoPostBack=\"True\" /> items per page");

            sb.AppendLine();
            sb.AppendLine("</asp:Panel>");

            if (!create_as_asp_control)
                sb.AppendLine("</asp:Content>");

            output.Body = sb.ToString();
            return output;
        }
        public static OutputObject GenerateWebListPageCodeBehind(SqlTable sql_table, List<string> namespace_includes)
        {
            if (sql_table == null)
                return null;

            string class_name = "List" + NameFormatter.ToCSharpPropertyName(sql_table.Name);

            OutputObject output = new OutputObject();
            output.Name = class_name + ".aspx.cs";
            output.Type = OutputObject.eObjectType.CSharp;

            namespace_includes.Add(NameFormatter.ToCSharpPropertyName(sql_table.Database.Name));

            StringBuilder sb = new StringBuilder();

            #region Includes
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Data;");
            sb.AppendLine("using System.Configuration;");
            sb.AppendLine("using System.Collections;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Web;");
            sb.AppendLine("using System.Web.Security;");
            sb.AppendLine("using System.Web.UI;");
            sb.AppendLine("using System.Web.UI.WebControls;");
            sb.AppendLine("using System.Web.UI.WebControls.WebParts;");
            sb.AppendLine("using System.Web.UI.HtmlControls;");
            sb.AppendLine();

            sb.AppendLine(GenerateNamespaceIncludes(namespace_includes));
            sb.AppendLine();

            #endregion

            sb.AppendLine("namespace WebControls." + NameFormatter.ToCSharpPropertyName(sql_table.Database.Name));
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "public partial class " + class_name + " : System.Web.UI.Page");
            sb.AppendLine(AddTabs(1) + "{");

            #region Fields
            sb.AppendLine(AddTabs(2) + "#region Fields");
            sb.AppendLine();
            sb.AppendLine(AddTabs(3) + "protected string _SQLConnection = ConfigurationManager.ConnectionStrings[\"SQLConnection\"].ConnectionString;");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#endregion");
            sb.AppendLine();
            #endregion

            #region Properties
            sb.AppendLine(AddTabs(2) + "#region Properties");
            sb.AppendLine();

            sb.AppendLine(AddTabs(3) + "protected string PageName");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "get { return \"" + class_name + "\"; }");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine(AddTabs(3) + "protected PaginationManager PageinationData");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "get");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "if (ViewState[\"PaginationManager\"] == null)");
            sb.AppendLine(AddTabs(6) + "ViewState[\"PaginationManager\"] = new PaginationManager();");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "return (PaginationManager)ViewState[\"PaginationManager\"];");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(4) + "set");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "ViewState[\"PaginationManager\"] = value;");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(3) + "}");

            sb.AppendLine(AddTabs(3) + "protected string ReturnPage");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "get");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "if (ViewState[\"ReturnPage\"] == null)");
            sb.AppendLine(AddTabs(6) + "ViewState[\"ReturnPage\"] = \"~/Default.aspx\";");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "return (string)ViewState[\"ReturnPage\"];");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(4) + "set");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "ViewState[\"ReturnPage\"] = value;");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(3) + "}");

            sb.AppendLine(AddTabs(3) + "protected int EditingID");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "get");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "if (ViewState[\"EditingID\"] == null)");
            sb.AppendLine(AddTabs(6) + "ViewState[\"EditingID\"] = -1;");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "return (int)ViewState[\"EditingID\"];");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(4) + "set");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "ViewState[\"EditingID\"] = value;");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(3) + "}");

            sb.AppendLine(AddTabs(3) + "protected string SearchString");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "get");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "if (ViewState[\"SearchString\"] == null)");
            sb.AppendLine(AddTabs(6) + "ViewState[\"SearchString\"] = string.Empty;");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "return (string)ViewState[\"SearchString\"];");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(4) + "set");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "ViewState[\"SearchString\"] = value;");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(3) + "}");

            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#endregion");
            sb.AppendLine();
            #endregion

            sb.AppendLine(AddTabs(2) + "#region Methods");
            sb.AppendLine();

            #region Bind Repeater method
            sb.AppendLine(AddTabs(3) + "protected void BindRepeater()");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "var dal_obj = new " + NameFormatter.ToCSharpClassName("Dal" + sql_table.Name) + "(_SQLConnection);");
            sb.AppendLine(AddTabs(4) + "dal_obj.LoadAllFromDbPaged(this.PageinationData.ItemIndex, this.PageinationData.PageSize, this.SearchString);");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "this.PageinationData.SetSize = dal_obj.GetSearchCountFromDb(this.SearchString);");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "rptList.DataSource = dal_obj.Collection;");
            sb.AppendLine(AddTabs(4) + "rptList.DataBind();");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            #region Bind Page Size Control method
            sb.AppendLine(AddTabs(3) + "protected void BindPageSizeControl()");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "this.ddlPageSize.Items.Clear();");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "this.ddlPageSize.Items.Add(new ListItem(\"10\"));");
            sb.AppendLine(AddTabs(4) + "this.ddlPageSize.Items.Add(new ListItem(\"25\"));");
            sb.AppendLine(AddTabs(4) + "this.ddlPageSize.Items.Add(new ListItem(\"50\"));");
            sb.AppendLine(AddTabs(4) + "this.ddlPageSize.Items.Add(new ListItem(\"100\"));");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "this.ddlPageSize.Text = this.PageinationData.PageSize.ToString();");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            #region Bind Page Counter method
            sb.AppendLine(AddTabs(3) + "protected void BindPageCounter()");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "this.lblCounter.Text = string.Format(\"Items {0} - {1} of {2}\",");
            sb.AppendLine(AddTabs(4) + "this.PageinationData.FirstItemOnCurrentPage, ");
            sb.AppendLine(AddTabs(4) + "this.PageinationData.LastItemOnCurrentPage, ");
            sb.AppendLine(AddTabs(4) + "this.PageinationData.SetSize);");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            #region Check Credentials method
            sb.AppendLine(AddTabs(3) + "protected void CheckCredentials()");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#endregion");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#region Events");
            sb.AppendLine();

            #region pageload event method
            sb.AppendLine(AddTabs(3) + "protected void Page_Load(object sender, EventArgs e)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "CheckCredentials();");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "this.txtSearch.Focus();");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "if (!Page.IsPostBack)");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "if (Request.QueryString[\"ReturnPage\"] != null)");
            sb.AppendLine(AddTabs(6) + "this.ReturnPage = (string)Request.QueryString[\"ReturnPage\"];");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "if (Request.QueryString[\"PageData\"] != null)");
            sb.AppendLine(AddTabs(6) + "this.PageinationData = new PaginationManager(Request.QueryString[\"PageData\"].ToString());");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "BindPageSizeControl();");
            sb.AppendLine(AddTabs(5) + "BindRepeater();");
            sb.AppendLine(AddTabs(5) + "BindPageCounter();");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            #region btnPrevPage_Click method
            sb.AppendLine(AddTabs(3) + "protected void btnPrevPage_Click(object sender, EventArgs e)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "this.PageinationData.PreviousPage();");
            sb.AppendLine(AddTabs(4) + "BindRepeater();");
            sb.AppendLine(AddTabs(4) + "BindPageCounter();");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            #region btnNextPage_Click method
            sb.AppendLine(AddTabs(3) + "protected void btnNextPage_Click(object sender, EventArgs e)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "this.PageinationData.NextPage();");
            sb.AppendLine(AddTabs(4) + "BindRepeater();");
            sb.AppendLine(AddTabs(4) + "BindPageCounter();");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            #region btnFirstPage_Click method
            sb.AppendLine(AddTabs(3) + "protected void btnFirstPage_Click(object sender, EventArgs e)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "this.PageinationData.JumpToFirstPage();");
            sb.AppendLine(AddTabs(4) + "BindRepeater();");
            sb.AppendLine(AddTabs(4) + "BindPageCounter();");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            #region btnLastPage_Click method
            sb.AppendLine(AddTabs(3) + "protected void btnLastPage_Click(object sender, EventArgs e)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "this.PageinationData.JumpToLastPage();");
            sb.AppendLine(AddTabs(4) + "BindRepeater();");
            sb.AppendLine(AddTabs(4) + "BindPageCounter();");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            #region btnAddNewItem_Click method
            sb.AppendLine(AddTabs(3) + "protected void btnAddNewItem_Click(object sender, EventArgs e)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "this.Response.Redirect(\"Edit" + NameFormatter.ToCSharpPropertyName(sql_table.Name) + ".Aspx?id=0&ReturnPage=\" + this.PageName + \"&PageData=\" + this.PageinationData.ToString(), true);");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            #region btnSearch_Click method
            sb.AppendLine(AddTabs(3) + "protected void btnSearch_Click(object sender, EventArgs e)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "this.SearchString = this.txtSearch.Text;");
            sb.AppendLine(AddTabs(4) + "this.PageinationData.ItemIndex = 1;");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "BindPageSizeControl();");
            sb.AppendLine(AddTabs(4) + "BindRepeater();");
            sb.AppendLine(AddTabs(4) + "BindPageCounter();");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            #region ddlPageSize_SelectedIndexChanged method
            sb.AppendLine(AddTabs(3) + "protected void ddlPageSize_SelectedIndexChanged(Object sender, EventArgs e)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "this.PageinationData.PageSize = Convert.ToInt32(this.ddlPageSize.Text);");
            sb.AppendLine(AddTabs(4) + "BindRepeater();");
            sb.AppendLine(AddTabs(4) + "BindPageCounter();");
            sb.AppendLine(AddTabs(3) + "}");
            #endregion

            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#endregion");
            sb.AppendLine(AddTabs(1) + "}");
            sb.AppendLine("}");
            sb.AppendLine();
            output.Body = sb.ToString();
            return output;
        }

        public static OutputObject GenerateDefaultPageCodeInFront(string database_name)
        {
            if (string.IsNullOrEmpty(database_name))
                return null;

            OutputObject output = new OutputObject();
            output.Name = ConfigurationManager.AppSettings["DefaultASPPageFilename"] + ".aspx";
            output.Type = OutputObject.eObjectType.Aspx;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<%@ Page Language=\"C#\" MasterPageFile=\"~/MasterPage.master\" AutoEventWireup=\"true\" CodeBehind=\"Default.aspx.cs\" Inherits=\"WebControls." + NameFormatter.ToCSharpPropertyName(database_name) + ".Default\" %>");
            sb.AppendLine();

            sb.AppendLine("<asp:Content ID=\"Content1\" ContentPlaceHolderID=\"head\" Runat=\"Server\">");
            sb.AppendLine("</asp:Content>");
            sb.AppendLine();

            sb.AppendLine("<asp:Content ID=\"Content2\" ContentPlaceHolderID=\"body\" Runat=\"Server\">");
            sb.AppendLine("</asp:Content>");
            sb.AppendLine();

            output.Body = sb.ToString();
            return output;
        }
        public static OutputObject GenerateDefaultPageCodeBehind(string database_name)
        {
            if (string.IsNullOrEmpty(database_name))
                return null;

            OutputObject output = new OutputObject();
            output.Name = ConfigurationManager.AppSettings["DefaultASPPageFilename"] + ".aspx.cs";
            output.Type = OutputObject.eObjectType.CSharp;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("using System;");
            sb.AppendLine("using System.Data;");
            sb.AppendLine("using System.Configuration;");
            sb.AppendLine("using System.Collections;");
            sb.AppendLine("using System.Web;");
            sb.AppendLine("using System.Web.Security;");
            sb.AppendLine("using System.Web.UI;");
            sb.AppendLine("using System.Web.UI.WebControls;");
            sb.AppendLine("using System.Web.UI.WebControls.WebParts;");
            sb.AppendLine("using System.Web.UI.HtmlControls;");
            sb.AppendLine();

            sb.AppendLine("namespace WebControls." + NameFormatter.ToCSharpPropertyName(database_name));
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "public partial class Default : System.Web.UI.Page");
            sb.AppendLine(AddTabs(1) + "{");
            sb.AppendLine(AddTabs(2) + "protected void Page_Load(object sender, EventArgs e)");
            sb.AppendLine(AddTabs(2) + "{");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "}");
            sb.AppendLine(AddTabs(1) + "}");
            sb.AppendLine("}");

            output.Body = sb.ToString();
            return output;
        }

        // Xml Export
        public static DataTable GenerateSqlTableData(string database_name, string table_name, string connection_string)
        {
            if (string.IsNullOrEmpty(database_name))
                throw new ArgumentException("database name is null or empty");

            if (string.IsNullOrEmpty(table_name))
                throw new ArgumentException("table name is null or empty");

            string SQL_Query = "SELECT * FROM [" + database_name + "].[dbo].[" + table_name + "]";
            DataTable data_table = Main.ExcecuteDbCall(SQL_Query, null, connection_string);

            data_table.TableName = table_name;

            return data_table;
        }
        public static OutputObject GenerateXmlLoader(SqlTable sql_table)
        {
            if (sql_table == null)
                return null;

            string object_name = NameFormatter.ToCSharpClassName(sql_table.Name);
            string class_name = object_name + "DataLoader";
            string collection_type = "List<" + object_name + ">";
            int longest_column = GetLongestColumnLength(sql_table) + sql_table.Name.Length;

            OutputObject output = new OutputObject();
            output.Name = class_name + ".cs";
            output.Type = OutputObject.eObjectType.CSharp;

            StringBuilder sb = new StringBuilder();

            #region Header block

            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Xml;");
            sb.AppendLine();
            sb.AppendLine("using DAL;");
            sb.AppendLine();

            #endregion

            sb.AppendLine("namespace " + NameFormatter.ToCSharpPropertyName(sql_table.Database.Name));
            sb.AppendLine("{");
            sb.AppendLine(AddTabs(1) + "[Serializable]");
            sb.AppendLine(AddTabs(1) + "public partial class " + class_name);
            sb.AppendLine(AddTabs(1) + "{");

            sb.AppendLine(AddTabs(2) + "#region Fields");
            sb.AppendLine();
            sb.AppendLine(AddTabs(3) + "protected bool _ParseErrors;");
            sb.AppendLine(AddTabs(3) + "protected " + collection_type + " _CollectionData;");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#endregion");
            sb.AppendLine();

            sb.AppendLine(AddTabs(2) + "#region Properties");
            sb.AppendLine();
            sb.AppendLine(AddTabs(3) + "public " + collection_type + " CollectionData");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "get { return _CollectionData; }");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#endregion");
            sb.AppendLine();

            sb.AppendLine(AddTabs(2) + "#region Methods");
            sb.AppendLine();

            sb.AppendLine(AddTabs(3) + "//CTOR");
            sb.AppendLine(AddTabs(3) + "public " + class_name + "()");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(3) + "}");
            sb.AppendLine();

            sb.AppendLine(AddTabs(3) + "public bool ParseData(string file_name)");
            sb.AppendLine(AddTabs(3) + "{");
            sb.AppendLine(AddTabs(4) + "string xml;");
            sb.AppendLine(AddTabs(4) + "string error_message;");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "_ParseErrors        = !FileIo.ReadFromFile(file_name, out xml, out error_message);");
            sb.AppendLine(AddTabs(4) + "_CollectionData     = new " + collection_type + "();");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "XmlDocument document;");
            sb.AppendLine(AddTabs(4) + "XmlNodeList selected_nodes;");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + object_name + " item_data;");
            sb.AppendLine();
            sb.AppendLine(AddTabs(4) + "try");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "document = new XmlDocument();");
            sb.AppendLine(AddTabs(5) + "document.InnerXml = xml;");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "selected_nodes = document.SelectNodes(\"//" + NameFormatter.ToCSharpPropertyName(sql_table.Name) + "\");");
            sb.AppendLine();
            sb.AppendLine(AddTabs(5) + "foreach (XmlNode item in selected_nodes)");
            sb.AppendLine(AddTabs(5) + "{");
            sb.AppendLine(AddTabs(6) + "item_data = new " + object_name + "();");
            sb.AppendLine();

            foreach (var sql_column in sql_table.Columns.Values)
            {
                // format: 
                // critter_data.Name       = item.Attributes["Name"].Value;
                sb.AppendLine(AddTabs(6) + PadCSharpVariableName("item_data." + NameFormatter.ToCSharpPropertyName(sql_column.Name), longest_column + 7) + "= " + NameFormatter.GetCSharpCastString(sql_column) + "(item.Attributes[\"" + NameFormatter.ToCSharpPropertyName(sql_column.Name) + "\"].Value);");
            }

            sb.AppendLine();
            sb.AppendLine(AddTabs(6) + "_CollectionData.Add(item_data);");
            sb.AppendLine(AddTabs(5) + "}");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(4) + "catch");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "_ParseErrors = true;");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine(AddTabs(4) + "finally");
            sb.AppendLine(AddTabs(4) + "{");
            sb.AppendLine(AddTabs(5) + "document        = null;");
            sb.AppendLine(AddTabs(5) + "selected_nodes  = null;");
            sb.AppendLine(AddTabs(4) + "}");
            sb.AppendLine();

            sb.AppendLine(AddTabs(3) + "return !_ParseErrors;");
            sb.AppendLine(AddTabs(2) + "}");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "#endregion");
            sb.AppendLine(AddTabs(1) + "}");

            sb.AppendLine("}");

            output.Body = sb.ToString();
            return output;
        }

        // Misc object Generation
        public static string GenerateGetTableDetailsStoredProcedure()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("USE MASTER");
            sb.AppendLine("GO");
            sb.AppendLine();

            sb.AppendLine("IF EXISTS (SELECT name FROM sysobjects WHERE name = 'sp_TableDetails' AND type = 'P')");
            sb.AppendLine("DROP PROCEDURE sp_TableDetails");
            sb.AppendLine("GO");
            sb.AppendLine();

            sb.AppendLine("/*");
            sb.AppendLine(AddTabs(1) + "sp_TableDetails");
            sb.AppendLine(AddTabs(1) + "By Roger Hill");
            sb.AppendLine(AddTabs(1) + "03/01/2007");
            sb.AppendLine();

            sb.AppendLine(AddTabs(1) + "Collates table column data into single");
            sb.AppendLine(AddTabs(1) + "table for easy use. BOOOYA!");
            sb.AppendLine("*/");
            sb.AppendLine();

            sb.AppendLine("CREATE PROCEDURE sp_TableDetails");
            sb.AppendLine("(");
            sb.AppendLine(AddTabs(1) + "@TableName	VARCHAR(255)");
            sb.AppendLine(")");
            sb.AppendLine("AS");
            sb.AppendLine();

            sb.AppendLine("SET NOCOUNT ON");
            sb.AppendLine();

            sb.AppendLine(GenerateGetTableDetailsLogic());

            sb.AppendLine("GO");

            return sb.ToString();
        }
        public static string GenerateGetTableDetailsLogic()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("DECLARE\t@VariableTypes TABLE ");
            sb.AppendLine("(");
            sb.AppendLine(AddTabs(1) + "[ColumnName] VARCHAR(255),");
            sb.AppendLine(AddTabs(1) + "[DataType] VARCHAR(255),");
            sb.AppendLine(AddTabs(1) + "[Length] INT,");
            sb.AppendLine(AddTabs(1) + "[Precision] INT,");
            sb.AppendLine(AddTabs(1) + "[Scale]INT,");
            sb.AppendLine(AddTabs(1) + "[isNullable] BIT,");
            sb.AppendLine(AddTabs(1) + "[isPK] BIT DEFAULT '0',");
            sb.AppendLine(AddTabs(1) + "[isIdentity] BIT DEFAULT '0'");
            sb.AppendLine(")");
            sb.AppendLine();

            sb.AppendLine("INSERT\t@VariableTypes");
            sb.AppendLine("(");
            sb.AppendLine(AddTabs(1) + "[ColumnName],");
            sb.AppendLine(AddTabs(1) + "[DataType],");
            sb.AppendLine(AddTabs(1) + "[Length],");
            sb.AppendLine(AddTabs(1) + "[Precision],");
            sb.AppendLine(AddTabs(1) + "[Scale],");
            sb.AppendLine(AddTabs(1) + "[isNullable]");
            sb.AppendLine(")");

            sb.AppendLine("SELECT\tsc.[Name] AS [ColumnName],");
            sb.AppendLine(AddTabs(1) + "st.[name] AS [DataType],");
            sb.AppendLine(AddTabs(1) + "sc.[length] AS [Length],");
            sb.AppendLine(AddTabs(1) + "sc.[xprec] AS [Precision],");
            sb.AppendLine(AddTabs(1) + "sc.[xScale] AS [Scale],");
            sb.AppendLine(AddTabs(1) + "sc.[isnullable]	AS [IsNullable]");
            sb.AppendLine("FROM\tsysobjects so");
            sb.AppendLine(AddTabs(1) + "INNER JOIN syscolumns sc ON so.[ID] = sc.[ID]");
            sb.AppendLine(AddTabs(1) + "INNER JOIN systypes st ON sc.[xtype] = st.[xtype]");
            sb.AppendLine("WHERE\tso.[name] = @TableName");
            sb.AppendLine("ORDER\tBY sc.colorder");
            sb.AppendLine();

            sb.AppendLine("-- Flag any Identities");
            sb.AppendLine("UPDATE\t@VariableTypes");
            sb.AppendLine("SET\t[isIdentity] = 1");
            sb.AppendLine("WHERE\t[ColumnName] IN");
            sb.AppendLine("(");
            sb.AppendLine(AddTabs(1) + "SELECT\tc.[name]");
            sb.AppendLine(AddTabs(1) + "FROM\tsyscolumns c ");
            sb.AppendLine(AddTabs(2) + "INNER JOIN sysobjects o ON o.[id] = c.[id]");
            sb.AppendLine(AddTabs(1) + "AND\tc.[autoval] IS NOT NULL");
            sb.AppendLine(AddTabs(1) + "AND\to.[name] = @TableName");
            sb.AppendLine(")");
            sb.AppendLine();

            sb.AppendLine("-- Flag any PKs in table");
            sb.AppendLine("UPDATE\t@VariableTypes");
            sb.AppendLine("SET\t[isPK] = 1");
            sb.AppendLine("WHERE\t[ColumnName] IN");
            sb.AppendLine("(");
            sb.AppendLine(AddTabs(1) + "SELECT\tColumnName = convert(SYSNAME,c.[name])");
            sb.AppendLine(AddTabs(1) + "FROM\tsysindexes i, ");
            sb.AppendLine(AddTabs(2) + "syscolumns c,");
            sb.AppendLine(AddTabs(2) + "sysobjects o");
            sb.AppendLine(AddTabs(1) + "WHERE\to.[id] = object_id(quotename(@TableName))");
            sb.AppendLine(AddTabs(1) + "AND\to.[id] = c.[id]");
            sb.AppendLine(AddTabs(1) + "AND\to.[id] = i.[id]");
            sb.AppendLine(AddTabs(1) + "AND\t(i.[status] & 0x800) = 0x800");
            sb.AppendLine(AddTabs(1) + "AND");
            sb.AppendLine(AddTabs(1) + "(");
            sb.AppendLine(AddTabs(2) + "c.[name] = index_col (quotename(@TableName), i.indid,  1) OR");
            sb.AppendLine(AddTabs(2) + "c.[name] = index_col (quotename(@TableName), i.indid,  2) OR");
            sb.AppendLine(AddTabs(2) + "c.[name] = index_col (quotename(@TableName), i.indid,  3) OR");
            sb.AppendLine(AddTabs(2) + "c.[name] = index_col (quotename(@TableName), i.indid,  4) OR");
            sb.AppendLine(AddTabs(2) + "c.[name] = index_col (quotename(@TableName), i.indid,  5) OR");
            sb.AppendLine(AddTabs(2) + "c.[name] = index_col (quotename(@TableName), i.indid,  6) OR");
            sb.AppendLine(AddTabs(2) + "c.[name] = index_col (quotename(@TableName), i.indid,  7) OR");
            sb.AppendLine(AddTabs(2) + "c.[name] = index_col (quotename(@TableName), i.indid,  8) OR");
            sb.AppendLine(AddTabs(2) + "c.[name] = index_col (quotename(@TableName), i.indid,  9) OR");
            sb.AppendLine(AddTabs(2) + "c.[name] = index_col (quotename(@TableName), i.indid, 10) OR");
            sb.AppendLine(AddTabs(2) + "c.[name] = index_col (quotename(@TableName), i.indid, 11) OR");
            sb.AppendLine(AddTabs(2) + "c.[name] = index_col (quotename(@TableName), i.indid, 12) OR");
            sb.AppendLine(AddTabs(2) + "c.[name] = index_col (quotename(@TableName), i.indid, 13) OR");
            sb.AppendLine(AddTabs(2) + "c.[name] = index_col (quotename(@TableName), i.indid, 14) OR");
            sb.AppendLine(AddTabs(2) + "c.[name] = index_col (quotename(@TableName), i.indid, 15) OR");
            sb.AppendLine(AddTabs(2) + "c.[name] = index_col (quotename(@TableName), i.indid, 16)");
            sb.AppendLine(AddTabs(1) + ")");
            sb.AppendLine(")");
            sb.AppendLine();

            sb.AppendLine("-- Return Data");
            sb.AppendLine("SELECT\t*");
            sb.AppendLine("FROM\t@VariableTypes");

            return sb.ToString();
        }

        // Script Header Generation
        public static string GenerateSqlScriptHeader(string database_name)
        {
            if (string.IsNullOrEmpty(database_name))
                throw new Exception("Cannot generate stored procedure name without a database name.");

            StringBuilder sb = new StringBuilder();

            // Script file header
            sb.Append("/* " + GenerateAuthorNotice() + " */");
            sb.Append(Environment.NewLine);
            sb.Append("USE [" + database_name + "]");
            sb.Append(Environment.NewLine + "GO");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);

            return sb.ToString();
        }
        public static string GenerateSqlStoredProcName(string table_name, eStoredProcType proc_type, List<string> select_fields)
        {
            // Sample:
            // sp_tb_lookup_events_SelectSingle

            if (string.IsNullOrEmpty(table_name))
                throw new Exception("Cannot generate stored procedure name without a table name.");

            string suffix = string.Empty;
            string selected_fields_string = string.Empty;

            // in the event that we are building a name dynamically
            if (select_fields != null)
            {
                foreach (var item in select_fields)
                    selected_fields_string += item;
            }

            switch (proc_type)
            {
                case eStoredProcType.SelectSingle: suffix = s_SelectSingleSpSuffix; break;
                case eStoredProcType.SelectMany: suffix = s_SelectManySpSuffix; break;
                case eStoredProcType.SelectManyByX: suffix = string.Format(s_SelectManyByXSpSuffix, selected_fields_string); break;
                case eStoredProcType.SelectAll: suffix = s_SelectAllSpSuffix; break;
                case eStoredProcType.SelectAllPag: suffix = s_SelectAllPagSpSuffix; break;
                case eStoredProcType.Insert: suffix = s_InsertSingleSpSuffix; break;
                case eStoredProcType.Update: suffix = s_UpdateSpSuffix; break;
                case eStoredProcType.UpdateInsert: suffix = s_UpdateInsertSpSuffix; break;
                case eStoredProcType.DelSingle: suffix = s_DelSingleSpSuffix; break;
                case eStoredProcType.DelMany: suffix = s_DelManySpSuffix; break;
                case eStoredProcType.DelAll: suffix = s_DelAllSpSuffix; break;
                case eStoredProcType.CountAll: suffix = s_CountAllSpSuffix; break;
                case eStoredProcType.CountSearch: suffix = s_CountSearchSpSuffix; break;

                default:
                    throw new Exception("StoredProcType unknown: " + proc_type.ToString());
            }

            //table_name = NameFormatter.ToTitleCase(table_name);
            table_name = table_name.Replace(" ", "_");
            table_name = s_SpNamePrefix + table_name + "_" + suffix;
            table_name = table_name.Replace("__", "_");

            return table_name;
        }
        public static string GenerateSqlStoredProcPerms(string stored_procedure_name)
        {
            if (string.IsNullOrEmpty(stored_procedure_name))
                throw new Exception("Cannot generate permissions without a stored procedure name.");

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("GRANT EXECUTE ON [dbo].[" + stored_procedure_name + "] TO [" + s_DefaultDbUserName + "]");
            sb.AppendLine("GO");

            return sb.ToString();
        }

        // helper script methods
        private static string GenerateAuthorNotice()
        {
            return "Generated by Jolly Roger's Autocode Generator on " + DateTime.Now.ToString();
        }
        private static string GenerateSqlExistanceChecker(string procedure_name)
        {
            if (string.IsNullOrEmpty(procedure_name))
                throw new Exception("Cannot generate tsql existance check without a database name.");

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("IF EXISTS (SELECT name FROM sysobjects WHERE name = '" + procedure_name + "' AND type = 'P')");
            sb.AppendLine("DROP PROCEDURE " + procedure_name);
            sb.Append("GO");

            return sb.ToString();
        }
        private static string GenerateHeader(string procedure_name)
        {
            if (string.IsNullOrEmpty(procedure_name))
                throw new Exception("Cannot generate procedure header without a procedure name.");

            StringBuilder sb = new StringBuilder();

            sb.Append("/*");
            sb.Append(Environment.NewLine + AddTabs(1) + procedure_name);
            sb.Append(Environment.NewLine + AddTabs(1) + GenerateAuthorNotice());
            sb.Append(Environment.NewLine + "*/");

            return sb.ToString();
        }
        private static string GenerateSqlStoredProcParameters(SqlTable sql_table, eIncludedFields included_fields)
        {
            if (sql_table == null)
                throw new Exception("Cannot generate stored procedure parameters without a sql table.");

            #region Sample Output
            //(
            //    @CountryID		INT,
            //    @CountryName		VARCHAR(255)
            //)
            #endregion

            StringBuilder sb = new StringBuilder();
            bool flag_first_value = true;
            int longest_column = GetLongestColumnLength(sql_table);

            sb.Append("(");

            // list query parameters
            foreach (var sql_column in sql_table.Columns.Values)
            {
                if ((included_fields == eIncludedFields.All) || (included_fields == eIncludedFields.PKOnly && sql_column.IsPk) ||
                    (included_fields == eIncludedFields.NoIdentities && !sql_column.IsIdentity))
                {
                    if (flag_first_value)
                        flag_first_value = false;
                    else
                        sb.Append(",");

                    sb.Append(Environment.NewLine + AddTabs(1));
                    sb.Append(PadSqlVariableName(NameFormatter.ToTSQLVariableName(sql_column), longest_column));
                    sb.Append(NameFormatter.ToTSQLType(sql_column));
                }
            }

            sb.Append(Environment.NewLine + ")");

            return sb.ToString();
        }
        private static string GenerateNamespaceIncludes(List<string> namespace_includes)
        {
            if (namespace_includes == null || namespace_includes.Count == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            namespace_includes.Sort();

            foreach (var item in namespace_includes)
            {
                string buffer = item.Trim();

                if (!string.IsNullOrEmpty(item))
                {
                    if (buffer.Contains(";"))
                        buffer = buffer.Replace(";", string.Empty);

                    sb.AppendLine(string.Format("using {0};", buffer));
                }
            }

            return sb.ToString();
        }
        private static string GenerateOrderByClause(SqlTable sql_table, List<string> column_names)
        {
            if (column_names == null || column_names.Count == 0)
                return "-- ORDER BY [<A_COLUMN_NAME>]";

            StringBuilder sb = new StringBuilder();

            sb.Append("ORDER BY ");

            bool flag_first_value = true;

            foreach (var item in column_names)
            {
                if (flag_first_value)
                    flag_first_value = false;
                else
                    sb.Append(",");

                sb.Append(NameFormatter.ToTSQLName(sql_table.Name) + "." + NameFormatter.ToTSQLName(item));
            }

            return sb.ToString();
        }
        private static string GenerateSearchClause(SqlTable sql_table, List<string> column_names, int tabs_in, int tabs_over)
        {
            if (sql_table == null || column_names == null || column_names.Count == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            bool flag_first_value = true;

            sb.Append(AddTabs(tabs_in) + "WHERE");
            sb.Append(Environment.NewLine + AddTabs(tabs_in) + "(");

            foreach (var item in column_names)
            {
                sb.Append(Environment.NewLine + AddTabs(tabs_in + 1));

                if (flag_first_value)
                    flag_first_value = false;
                else
                    sb.Append("OR ");

                sb.Append(NameFormatter.ToTSQLName(sql_table.Name) + "." + NameFormatter.ToTSQLName(item) + " LIKE '%' + @SearchString + '%'");
            }
            sb.Append(Environment.NewLine + AddTabs(tabs_in) + ")");

            return sb.ToString();
        }
        private static string GenerateSelectClause(SqlTable sql_table, List<string> column_names)
        {
            #region Sample output:
            // AND [UserId]     = @UserId
            // AND [Age]        = @Age
            // AND [ShoeSize]   = @ShoeSize
            #endregion

            if (sql_table == null || column_names == null || column_names.Count == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            bool flag_first_value = true;
            int longest_column = GetLongestColumnLength(sql_table) + sql_table.Name.Length;

            foreach (var item in column_names)
            {
                string field_name = NameFormatter.ToTSQLName(sql_table.Name) + "." + NameFormatter.ToTSQLName(item);

                if (flag_first_value)
                    flag_first_value = false;
                else
                    sb.Append(Environment.NewLine);

                sb.Append("AND" + AddTabs(2) + PadSqlVariableName(field_name, longest_column) + "= " + NameFormatter.ToTSQLVariableName(sql_table.Columns[item]));
            }

            return sb.ToString();
        }
        private static string GenerateSelectClauseArguments(SqlTable sql_table, List<string> column_names)
        {
            if (column_names == null)
                return AddTabs(1) + "-- @A_VALUE [SOME_DATA_TYPE]";

            StringBuilder sb = new StringBuilder();

            bool flag_first_value = true;

            foreach (var item in column_names)
            {
                if (flag_first_value)
                    flag_first_value = false;
                else
                    sb.Append("," + Environment.NewLine);

                sb.Append(AddTabs(1) + NameFormatter.ToTSQLVariableName(sql_table.Columns[item]) + AddTabs(2) + NameFormatter.ToTSQLType(sql_table.Columns[item]));
            }

            return sb.ToString();
        }
        private static string GenerateSqlListCode()
        {
            // This is the code that takes a single sql text field and 
            // breaks it up to use as a list of int varables.

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("DECLARE @IDListPosition INT");
            sb.AppendLine("DECLARE @ArrValue VARCHAR(MAX)");
            sb.AppendLine();
            sb.AppendLine("DECLARE @TableVar TABLE");
            sb.AppendLine("(");
            sb.AppendLine(AddTabs(1) + "IdValue VARCHAR(50) NOT NULL");
            sb.AppendLine(")");
            sb.AppendLine();
            sb.AppendLine("SET @IDList = COALESCE(@IDList ,'')");
            sb.AppendLine();
            sb.AppendLine("IF @IDList <> ''");
            sb.AppendLine("BEGIN");
            sb.AppendLine();
            sb.AppendLine(AddTabs(1) + "SET @IDList = @IDList + ','");
            sb.AppendLine(AddTabs(1) + "WHILE PATINDEX('%,%', @IDList) <> 0");
            sb.AppendLine();
            sb.AppendLine(AddTabs(1) + "BEGIN");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "SELECT @IDListPosition = PATINDEX('%,%', @IDList)");
            sb.AppendLine(AddTabs(2) + "SELECT @ArrValue = left(@IDList, @IDListPosition - 1)");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "INSERT INTO @TableVar (IdValue) VALUES (@ArrValue)");
            sb.AppendLine();
            sb.AppendLine(AddTabs(2) + "SELECT @IDList = stuff(@IDList, 1, @IDListPosition, '')");
            sb.AppendLine();
            sb.AppendLine(AddTabs(1) + "END");
            sb.AppendLine("END");

            return sb.ToString();
        }
        private static string FindIdField(SqlTable sql_table)
        {
            // occasionally we might need to figure out a primary key for a table.
            // this method isn't perfect, but its a decent stab at the problem.
            // grab the first PK we have. If there is no PKs defined, grab the
            // first int we find.

            if (sql_table.PkList.Count == 0)
            {
                foreach (var column in sql_table.Columns)
                {
                    if (column.Value.BaseType == eSqlBaseType.Integer)
                        return column.Key;
                }

                // still here? no matches then...
                return string.Empty;
            }
            else
            {
                return sql_table.PkList[0].Name;
            }
        }
        private static string FindNameField(SqlTable sql_table)
        {
            // occasionally we might need to figure out a friendly name field for 
            // a table. this method isn't perfect, but its a decent stab at the problem.
            // grab the first text field we have, and hope for the best.

            foreach (var column in sql_table.Columns)
            {
                if (column.Value.BaseType == eSqlBaseType.String)
                    return column.Key;
            }

            // still here? no matches then...
            return string.Empty;
        }

        // formatting methods
        private static string AddTabs(int tab_count)
        {
            string tab_string = string.Empty;

            while (tab_count > 0)
            {
                tab_string += "\t";
                tab_count--;
            }

            return tab_string;
        }
        private static int GetLongestColumnLength(DataTable dt, int column_number)
        {
            int longest_length = 0;
            int current_length = 0;

            foreach (DataRow dr in dt.Rows)
            {
                current_length = dr[column_number].ToString().Length;

                if (current_length > longest_length)
                    longest_length = current_length;
            }

            return longest_length;
        }
        private static int GetLongestColumnLength(SqlTable sql_table)
        {
            if (sql_table == null)
                throw new Exception("Cannot compute longest column length on a null table.");

            int longest_column_length = 0;

            foreach (var column in sql_table.Columns.Values)
            {
                if (column.Name.Length > longest_column_length)
                    longest_column_length = column.Name.Length;
            }

            return longest_column_length;
        }
        private static string PadCSharpVariableName(string input, int longest_string_length)
        {
            return PadVariableName(input, longest_string_length, 0, s_VisualStudioTabSize);
        }
        private static string PadCSharpVariableName(string input, int longest_string_length, int padding)
        {
            return PadVariableName(input, longest_string_length, padding, s_VisualStudioTabSize);
        }
        private static string PadSqlVariableName(string input, int longest_string_length)
        {
            return PadVariableName(input, longest_string_length, 0, s_SQLTabSize);
        }
        private static string PadSqlVariableName(string input, int longest_string_length, int padding)
        {
            return PadVariableName(input, longest_string_length, padding, s_SQLTabSize);
        }
        private static string PadVariableName(string input, int longest_string_length, int additional_characters, int tab_size)
        {
            longest_string_length += tab_size + additional_characters;
            int pad_length = longest_string_length + (tab_size - (longest_string_length % tab_size));
            return input.PadRight(pad_length, ' ');
        }

        // Asp.net
        private static string GenerateRegexValidator(eSqlBaseType base_type, string control_ID, string control_to_validate)
        {
            string is_bool = @"^([0-1]|true|false)$";
            string is_int = @"[-+]?[0-9]{1,10}";
            string is_float = @"^[-+]?[0-9.]+$";
            string is_guid = @"^[0-9a-fA-F]{8}[-]?[0-9a-fA-F]{4}[-]?[0-9a-fA-F]{4}[-]?[0-9a-fA-F]{4}[-]?[0-9a-fA-F]{12}$";


            string regex;

            switch (base_type)
            {
                case eSqlBaseType.Bool:
                    regex = "<asp:RegularExpressionValidator ID=\"" + control_ID + "\" ControlToValidate=\"" + control_to_validate + "\" ErrorMessage=\"Value is not an boolean value\" ValidationExpression=\"" + is_bool + "\" runat=\"server\" />";
                    break;

                case eSqlBaseType.Integer:
                    regex = "<asp:RegularExpressionValidator ID=\"" + control_ID + "\" ControlToValidate=\"" + control_to_validate + "\" ErrorMessage=\"Value is not an integer value\" ValidationExpression=\"" + is_int + "\" runat=\"server\" />";
                    break;

                case eSqlBaseType.Float:
                    regex = "<asp:RegularExpressionValidator ID=\"" + control_ID + "\" ControlToValidate=\"" + control_to_validate + "\" ErrorMessage=\"Value is not a decimal value\" ValidationExpression=\"" + is_float + "\" runat=\"server\" />";
                    break;

                case eSqlBaseType.Guid:
                    regex = "<asp:RegularExpressionValidator ID=\"" + control_ID + "\" ControlToValidate=\"" + control_to_validate + "\" ErrorMessage=\"Value is not a GUID\" ValidationExpression=\"" + is_guid + "\" runat=\"server\" />";
                    break;

                // DateTimeRegex isn't good enough yet        
                //case BaseType.Time:
                //regex = "<asp:RegularExpressionValidator ID=\"" + control_ID + "\" ControlToValidate=\"" + control_to_validate + "\" ErrorMessage=\"Value is not a valid date time\" ValidationExpression=\"" + RegularExpression.s_DateTime + "\" runat=\"server\" />"; 
                //break;

                default:
                    regex = string.Empty;
                    break;
            }

            return regex;
        }
    }
}