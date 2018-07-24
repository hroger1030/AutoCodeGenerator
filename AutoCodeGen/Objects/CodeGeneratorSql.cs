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
using System.Linq;

namespace AutoCodeGenLibrary
{
    public partial class CodeGenerator : CodeGeneratorBase
    {
        private static string _SpNamePrefix = ConfigurationManager.AppSettings["SpNamePrefix"];
        private static string _SelectSingleByXSpSuffix = ConfigurationManager.AppSettings["SelectSingleByXSpSuffix"];
        private static string _SelectManySpSuffix = ConfigurationManager.AppSettings["SelectManySpSuffix"];
        private static string _SelectManyByXSpSuffix = ConfigurationManager.AppSettings["SelectManyByXSpSuffix"];
        private static string _SelectAllSpSuffix = ConfigurationManager.AppSettings["SelectAllSpSuffix"];
        private static string _SelectAllPagSpSuffix = ConfigurationManager.AppSettings["SelectAllPagSpSuffix"];
        private static string _InsertSingleSpSuffix = ConfigurationManager.AppSettings["InsertSingleSpSuffix"];
        private static string _UpdateSpSuffix = ConfigurationManager.AppSettings["UpdateSpSuffix"];
        private static string _UpdateInsertSpSuffix = ConfigurationManager.AppSettings["UpdateInsertSpSuffix"];
        private static string _DelAllSpSuffix = ConfigurationManager.AppSettings["DelAllSpSuffix"];
        private static string _DelManySpSuffix = ConfigurationManager.AppSettings["DelManySpSuffix"];
        private static string _DelSingleSpSuffix = ConfigurationManager.AppSettings["DelSingleSpSuffix"];
        private static string _CountAllSpSuffix = ConfigurationManager.AppSettings["CountAllSpSuffix"];
        private static string _CountSearchSpSuffix = ConfigurationManager.AppSettings["CountSearchSpSuffix"];

        private static string _DefaultDbUserName = ConfigurationManager.AppSettings["DefaultDbUserName"];
        private static int _SQLTabSize = Convert.ToInt32(ConfigurationManager.AppSettings["SQLTabSize"]);

        public static OutputObject GenerateSelectSingleProc(SqlTable sqlTable, bool generateStoredProcPerms, bool includeDisabledCheck)
        {
            if (sqlTable == null)
                return null;

            // todo: add in pk name list

            string procedure_name = GenerateSqlStoredProcName(sqlTable.Name, eStoredProcType.SelectSingle, sqlTable.PkNames);
            bool first_flag;
            string join_conjunction;

            var output = new OutputObject();
            output.Name = procedure_name + ".sql";
            output.Type = OutputObject.eObjectType.Sql;

            // sanity check - if there are no primary keys in the table, we cannot know 
            // what would a good choice would be for a qualifier. Abort and return error msg.
            if (sqlTable.PkList.Count == 0)
            {
                output.Body = $"/* Cannot generate {procedure_name}, no primary keys set on {sqlTable.Name}. */" + Environment.NewLine + Environment.NewLine;
                return output;
            }

            var sb = new StringBuilder();

            sb.AppendLine(GenerateSqlSpExistanceChecker(procedure_name));
            sb.AppendLine();
            sb.Append(GenerateDescriptionHeader(procedure_name));

            sb.AppendLine($"CREATE PROCEDURE [dbo].[{procedure_name}]");
            sb.AppendLine(GenerateSqlStoredProcParameters(sqlTable, eIncludedFields.PKOnly));

            sb.AppendLine("AS");
            sb.AppendLine();
            sb.AppendLine("SET NOCOUNT ON");
            sb.AppendLine();

            #region Selected Columns

            first_flag = true;

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if (first_flag)
                {
                    sb.Append("SELECT" + AddTabs(1));
                    first_flag = false;
                }
                else
                {
                    sb.Append("," + Environment.NewLine + AddTabs(2));
                }

                sb.Append($"{NameFormatter.ToTSQLName(sql_column.Table.Name)}.{NameFormatter.ToTSQLName(sql_column.Name)}");
            }
            #endregion

            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("FROM" + AddTabs(1) + NameFormatter.ToTSQLName(sqlTable.Name));
            sb.AppendLine();

            #region Where Clause

            first_flag = true;
            join_conjunction = "WHERE" + AddTabs(1);

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                string tsql_variablename = NameFormatter.ToTSQLVariableName(sql_column);

                if (sql_column.IsPk)
                    sb.AppendLine($"{join_conjunction}{NameFormatter.ToTSQLName(sql_column.Table.Name)}.{NameFormatter.ToTSQLName(sql_column.Name)} = {tsql_variablename}");

                if (first_flag)
                {
                    first_flag = false;
                    join_conjunction = "AND" + AddTabs(2);
                }
            }

            if (includeDisabledCheck)
                sb.AppendLine($"{join_conjunction}{NameFormatter.ToTSQLName(sqlTable.Name)}.[Disabled] = 0");

            #endregion

            sb.AppendLine("GO");
            sb.AppendLine();

            if (generateStoredProcPerms)
            {
                sb.AppendLine(GenerateSqlStoredProcPerms(procedure_name));
                sb.AppendLine();
            }

            output.Body = sb.ToString();
            return output;
        }

        public static OutputObject GenerateSelectManyProc(SqlTable sqlTable, List<string> sortFields, bool generateStoredProcPerms)
        {
            if (sqlTable == null)
                return null;

            string procedure_name = GenerateSqlStoredProcName(sqlTable.Name, eStoredProcType.SelectMany, null);
            string table_type_name = $"Tbl{procedure_name}";
            int longest_column = GetLongestColumnLength(sqlTable) + sqlTable.Name.Length;
            bool flag_first_value;
            string name_buffer;

            OutputObject output = new OutputObject();
            output.Name = procedure_name + ".sql";
            output.Type = OutputObject.eObjectType.Sql;

            if (sqlTable.PkList.Count == 0)
            {
                output.Body = $"/* Cannot generate {procedure_name}, no primary keys set on {sqlTable.Name}. */" + Environment.NewLine + Environment.NewLine;
                return output;
            }

            var sb = new StringBuilder();

            // need to create table type for SP
            sb.AppendLine(GenerateSqlTableTypeExistanceChecker("TblIdList"));
            sb.AppendLine();

            var hack = new StringBuilder();
            hack.AppendLine("--temp hack");
            hack.AppendLine("CREATE TYPE [dbo].[TblIdList] AS TABLE");
            hack.AppendLine("(");
            hack.AppendLine("[Id] [int] NULL");
            hack.AppendLine(")");
            hack.AppendLine("GO");

            sb.AppendLine(hack.ToString());
            sb.AppendLine();


            //ALTER PROCEDURE[dbo].[Card_SelectByTags]
            //(
            //    @Skip INT,
            //    @Take INT
            //)
            //AS

            //SET NOCOUNT ON

            //SELECT*

            //FROM[Card] c
            //WHERE[CardId] IN
            //(
            //SELECT[Id] FROM @AndTagIds
            //)

            //ORDER BY[Name]

            // now on to SP code
            sb.AppendLine(GenerateSqlSpExistanceChecker(procedure_name));
            sb.AppendLine(GenerateDescriptionHeader(procedure_name));

            sb.AppendLine($"CREATE PROCEDURE [dbo].[{procedure_name}]");
            sb.AppendLine("(");
            sb.AppendLine(AddTabs(1) + "@IdList VARCHAR(MAX)");
            sb.AppendLine(")");
            sb.AppendLine("AS");
            sb.AppendLine();
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
            sb.AppendLine(GenerateOrderByClause(sqlTable, sortFields));
            sb.AppendLine("GO");
            sb.AppendLine();

            if (generateStoredProcPerms)
            {
                sb.AppendLine(GenerateSqlStoredProcPerms(procedure_name));
                sb.AppendLine();
            }

            output.Body = sb.ToString();
            return output;
        }

        public static OutputObject GenerateSelectManyByXProc(SqlTable sqlTable, List<string> sort_fields, List<string> select_fields, bool generateStoredProcPerms)
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

            var sb = new StringBuilder();

            sb.AppendLine(GenerateSqlSpExistanceChecker(procedure_name));
            sb.AppendLine(GenerateDescriptionHeader(procedure_name));

            sb.AppendLine("CREATE PROCEDURE " + procedure_name);
            sb.AppendLine("(");
            sb.AppendLine(GenerateSelectClauseArguments(sqlTable, select_fields));
            sb.AppendLine(")");
            sb.AppendLine("AS");
            sb.AppendLine();
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

            if (generateStoredProcPerms)
            {
                sb.AppendLine(GenerateSqlStoredProcPerms(procedure_name));
                sb.AppendLine();
            }

            output.Body = sb.ToString();
            return output;
        }

        public static OutputObject GenerateSelectAllProc(SqlTable sqlTable, List<string> sort_fields, bool generateStoredProcPerms)
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

            var sb = new StringBuilder();

            sb.AppendLine(GenerateSqlSpExistanceChecker(procedure_name));
            sb.AppendLine(GenerateDescriptionHeader(procedure_name));

            sb.AppendLine("CREATE PROCEDURE " + procedure_name);
            sb.AppendLine("AS");
            sb.AppendLine();
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

            if (generateStoredProcPerms)
            {
                sb.AppendLine(GenerateSqlStoredProcPerms(procedure_name));
                sb.AppendLine();
            }

            output.Body = sb.ToString();
            return output;
        }

        public static OutputObject GenerateSelectAllPaginatedProc(SqlTable sqlTable, List<string> sortFields, List<string> searchFields, bool generateStoredProcPerms)
        {
            if (sqlTable == null)
                return null;

            string procedure_name = GenerateSqlStoredProcName(sqlTable.Name, eStoredProcType.SelectAllPag, null);
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
                output.Body = $"/* Cannot generate {procedure_name}, no primary keys set on {sqlTable.Name}. */" + Environment.NewLine + Environment.NewLine;
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

            var sb = new StringBuilder();

            sb.AppendLine(GenerateSqlSpExistanceChecker(procedure_name));
            sb.AppendLine(GenerateDescriptionHeader(procedure_name));

            sb.AppendLine($"CREATE PROCEDURE [{procedure_name}]");
            sb.AppendLine("(");
            sb.AppendLine(AddTabs(1) + "@Skip INT,");
            sb.AppendLine(AddTabs(1) + "@Take INT,");
            sb.AppendLine(AddTabs(1) + "@SearchString VARCHAR(50)");
            sb.AppendLine(")");

            sb.AppendLine("AS");
            sb.AppendLine();

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
            sb.AppendLine(GenerateSearchClause(sqlTable, searchFields, 0, 1));
            sb.AppendLine(AddTabs(1) + "AND" + AddTabs(1) + NameFormatter.ToTSQLName(sqlTable.Name) + ".[Disabled] = 0");
            sb.AppendLine();
            sb.AppendLine(GenerateOrderByClause(sqlTable, sortFields));
            sb.AppendLine("OFFSET	@Skip ROWS FETCH NEXT @Take ROWS ONLY");
            sb.AppendLine("GO");
            sb.AppendLine();

            if (generateStoredProcPerms)
            {
                sb.AppendLine(GenerateSqlStoredProcPerms(procedure_name));
                sb.AppendLine();
            }

            output.Body = sb.ToString();
            return output;
        }

        public static OutputObject GenerateSetProc(SqlTable sqlTable, bool generateStoredProcPerms)
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
                output.Body = $"/* Cannot generate {procedure_name}, no primary keys set on {sqlTable.Name}. */" + Environment.NewLine + Environment.NewLine;
                return output;
            }

            var sb = new StringBuilder();

            sb.AppendLine(GenerateSqlSpExistanceChecker(procedure_name));
            sb.AppendLine(GenerateDescriptionHeader(procedure_name));

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

            if (generateStoredProcPerms)
            {
                sb.AppendLine(GenerateSqlStoredProcPerms(procedure_name));
                sb.AppendLine();
            }

            output.Body = sb.ToString();
            return output;
        }

        public static OutputObject GenerateDeleteSingleProc(SqlTable sqlTable, bool generateStoredProcPerms)
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
                output.Body = $"/* Cannot generate {procedure_name}, no primary keys set on {sqlTable.Name}. */" + Environment.NewLine + Environment.NewLine;
                return output;
            }

            var sb = new StringBuilder();

            sb.AppendLine(GenerateSqlSpExistanceChecker(procedure_name));
            sb.AppendLine(GenerateDescriptionHeader(procedure_name));

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

            if (generateStoredProcPerms)
            {
                sb.AppendLine(GenerateSqlStoredProcPerms(procedure_name));
                sb.AppendLine();
            }

            output.Body = sb.ToString();
            return output;
        }

        public static OutputObject GenerateDeleteManyProc(SqlTable sqlTable, bool generateStoredProcPerms)
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
                output.Body = $"/* Cannot generate {procedure_name}, no primary keys set on {sqlTable.Name}. */" + Environment.NewLine + Environment.NewLine;
                return output;
            }

            var sb = new StringBuilder();

            sb.AppendLine(GenerateSqlSpExistanceChecker(procedure_name));
            sb.AppendLine(GenerateDescriptionHeader(procedure_name));

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

            if (generateStoredProcPerms)
            {
                sb.AppendLine(GenerateSqlStoredProcPerms(procedure_name));
                sb.AppendLine();
            }

            output.Body = sb.ToString();
            return output;
        }

        public static OutputObject GenerateDeleteAllProc(SqlTable sqlTable, bool generateStoredProcPerms)
        {
            if (sqlTable == null)
                return null;

            string procedure_name = GenerateSqlStoredProcName(sqlTable.Name, eStoredProcType.DelAll, null);

            OutputObject output = new OutputObject();
            output.Name = procedure_name + ".sql";
            output.Type = OutputObject.eObjectType.Sql;

            var sb = new StringBuilder();

            sb.AppendLine(GenerateSqlSpExistanceChecker(procedure_name));
            sb.AppendLine(GenerateDescriptionHeader(procedure_name));

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

            if (generateStoredProcPerms)
            {
                sb.AppendLine(GenerateSqlStoredProcPerms(procedure_name));
                sb.AppendLine();
            }

            output.Body = sb.ToString();
            return output;
        }

        public static OutputObject GenerateCountAllProc(SqlTable sqlTable, bool generateStoredProcPerms)
        {
            if (sqlTable == null)
                return null;

            string procedure_name = GenerateSqlStoredProcName(sqlTable.Name, eStoredProcType.CountAll, null);

            OutputObject output = new OutputObject();
            output.Name = procedure_name + ".sql";
            output.Type = OutputObject.eObjectType.Sql;

            var sb = new StringBuilder();

            sb.AppendLine(GenerateSqlSpExistanceChecker(procedure_name));
            sb.AppendLine(GenerateDescriptionHeader(procedure_name));

            sb.AppendLine("CREATE PROCEDURE " + procedure_name);
            sb.AppendLine("AS");
            sb.AppendLine();
            sb.AppendLine("SET NOCOUNT ON");
            sb.AppendLine();
            sb.AppendLine("SELECT\tCOUNT(*)");
            sb.AppendLine("FROM\t[" + sqlTable.Name + "]");
            sb.AppendLine("WHERE\t[Disabled] = 0");
            sb.AppendLine("GO");
            sb.AppendLine();

            if (generateStoredProcPerms)
            {
                sb.AppendLine(GenerateSqlStoredProcPerms(procedure_name));
                sb.AppendLine();
            }

            output.Body = sb.ToString();
            return output;
        }

        public static OutputObject GenerateCountSearchProc(SqlTable sqlTable, List<string> search_fields, bool generateStoredProcPerms)
        {
            if (sqlTable == null)
                return null;

            string procedure_name = GenerateSqlStoredProcName(sqlTable.Name, eStoredProcType.CountSearch, null);

            OutputObject output = new OutputObject();
            output.Name = procedure_name + ".sql";
            output.Type = OutputObject.eObjectType.Sql;

            var sb = new StringBuilder();

            sb.AppendLine(GenerateSqlSpExistanceChecker(procedure_name));
            sb.AppendLine(GenerateDescriptionHeader(procedure_name));

            sb.AppendLine("CREATE PROCEDURE " + procedure_name);
            sb.AppendLine("(");
            sb.AppendLine(AddTabs(1) + "@SearchString VARCHAR(50)");
            sb.AppendLine(")");
            sb.AppendLine("AS");
            sb.AppendLine();
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

            if (generateStoredProcPerms)
            {
                sb.AppendLine(GenerateSqlStoredProcPerms(procedure_name));
                sb.AppendLine();
            }

            output.Body = sb.ToString();
            return output;
        }

        // Misc object Generation
        public static string GenerateGetTableDetailsStoredProcedure()
        {
            var sb = new StringBuilder();

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
            var sb = new StringBuilder();

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

            var sb = new StringBuilder();

            // Script file header
            sb.Append("/* " + GenerateAuthorNotice() + " */");
            sb.Append(Environment.NewLine);
            sb.Append("USE [" + database_name + "]");
            sb.Append(Environment.NewLine + "GO");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);

            return sb.ToString();
        }

        protected static string GenerateSqlStoredProcName(string tableName, eStoredProcType procType, IEnumerable<string> selectedFields)
        {
            // Sample:
            // [dbo].[Events_SelectSingle]

            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentException("Cannot generate stored procedure name without a table name.");

            string suffix = string.Empty;
            string selected_fields_string = (selectedFields == null) ? string.Empty : String.Join(string.Empty, selectedFields);

            switch (procType)
            {
                case eStoredProcType.SelectSingle: suffix = string.Format(_SelectSingleByXSpSuffix, selected_fields_string); break;
                case eStoredProcType.SelectMany: suffix = _SelectManySpSuffix; break;
                case eStoredProcType.SelectManyByX: suffix = string.Format(_SelectManyByXSpSuffix, selected_fields_string); break;
                case eStoredProcType.SelectAll: suffix = _SelectAllSpSuffix; break;
                case eStoredProcType.SelectAllPag: suffix = _SelectAllPagSpSuffix; break;
                case eStoredProcType.Insert: suffix = _InsertSingleSpSuffix; break;
                case eStoredProcType.Update: suffix = _UpdateSpSuffix; break;
                case eStoredProcType.UpdateInsert: suffix = _UpdateInsertSpSuffix; break;
                case eStoredProcType.DelSingle: suffix = _DelSingleSpSuffix; break;
                case eStoredProcType.DelMany: suffix = _DelManySpSuffix; break;
                case eStoredProcType.DelAll: suffix = _DelAllSpSuffix; break;
                case eStoredProcType.CountAll: suffix = _CountAllSpSuffix; break;
                case eStoredProcType.CountSearch: suffix = _CountSearchSpSuffix; break;

                default:
                    throw new Exception($"StoredProcType unknown: {procType}");
            }

            // tried title case here, gets a little odd.
            tableName = tableName.Replace(" ", "_");
            tableName = $"{_SpNamePrefix}{tableName}_{suffix}";
            tableName = tableName.Replace("__", "_");

            return tableName;
        }

        public static string GenerateSqlStoredProcPerms(string stored_procedure_name)
        {
            if (string.IsNullOrEmpty(stored_procedure_name))
                throw new Exception("Cannot generate permissions without a stored procedure name.");

            var sb = new StringBuilder();

            sb.AppendLine("GRANT EXECUTE ON [dbo].[" + stored_procedure_name + "] TO [" + _DefaultDbUserName + "]");
            sb.AppendLine("GO");

            return sb.ToString();
        }

        // helper script methods

        private static string GenerateSqlSpExistanceChecker(string procedureName)
        {
            if (string.IsNullOrEmpty(procedureName))
                throw new Exception("Cannot generate sql existance check without a database name.");

            var sb = new StringBuilder();

            sb.AppendLine($"IF EXISTS (SELECT name FROM sys.objects WHERE name = '{procedureName}' AND type = 'P')");
            sb.AppendLine($"DROP PROCEDURE [dbo].[{procedureName}]");
            sb.Append("GO");

            return sb.ToString();
        }

        private static string GenerateSqlTableTypeExistanceChecker(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
                throw new Exception("Cannot generate sql existance check without a database name.");

            var sb = new StringBuilder();

            sb.AppendLine($"IF EXISTS (SELECT name FROM sys.table_types WHERE name = '{tableName}' AND is_table_type = 1)");
            sb.AppendLine($"DROP TYPE [dbo].[{tableName}]");
            sb.Append("GO");

            return sb.ToString();
        }

        private static string GenerateDescriptionHeader(string procedureName)
        {
            if (string.IsNullOrEmpty(procedureName))
                throw new ArgumentException("Cannot generate procedure header without a procedure name.");

            var sb = new StringBuilder();

            sb.AppendLine("/*");
            sb.AppendLine($"{AddTabs(1)}{procedureName}");
            sb.AppendLine($"{AddTabs(1)}EXEC [dbo].[{procedureName}] -- todo need arg defaults");
            sb.AppendLine(AddTabs(1) + GenerateAuthorNotice());
            sb.AppendLine("*/");

            return sb.ToString();
        }

        private static string GenerateSqlStoredProcParameters(SqlTable sqlTable, eIncludedFields includedFields)
        {
            if (sqlTable == null)
                throw new ArgumentException("Cannot generate stored procedure parameters without a sql table.");

            #region Sample Output
            //(
            //    @CountryID INT,
            //    @CountryName VARCHAR(255)
            //)
            #endregion

            var sb = new StringBuilder();
            bool flag_first_value = true;
            int longest_column = GetLongestColumnLength(sqlTable);

            sb.Append("(");

            // list query parameters
            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if ((includedFields == eIncludedFields.All) ||
                    (includedFields == eIncludedFields.PKOnly && sql_column.IsPk) ||
                    (includedFields == eIncludedFields.NoIdentities && !sql_column.IsIdentity))
                {
                    if (flag_first_value)
                        flag_first_value = false;
                    else
                        sb.Append(",");

                    sb.Append(Environment.NewLine + AddTabs(1));
                    sb.Append(NameFormatter.ToTSQLVariableName(sql_column));
                    sb.Append(" ");
                    sb.Append(NameFormatter.ToTSQLType(sql_column));
                }
            }

            sb.Append(Environment.NewLine + ")");

            return sb.ToString();
        }

        private static string GenerateOrderByClause(SqlTable sql_table, List<string> column_names)
        {
            if (column_names == null || column_names.Count == 0)
                return "-- ORDER BY [<A_COLUMN_NAME>]";

            var sb = new StringBuilder();

            sb.Append("ORDER    BY ");

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

            var sb = new StringBuilder();

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

            var sb = new StringBuilder();

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

            var sb = new StringBuilder();

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

            var sb = new StringBuilder();

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



        // formatting methods
        private static string PadSqlVariableName(string input, int longest_string_length)
        {
            return PadVariableName(input, longest_string_length, 0, _SQLTabSize);
        }
    }
}