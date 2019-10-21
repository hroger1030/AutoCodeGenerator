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
using System.Linq;
using System.Text;

using DAL.Standard.SqlMetadata;

namespace AutoCodeGenLibrary
{
    public class CodeGeneratorSql : CodeGeneratorBase, IGenerator
    {
        private readonly string _DefaultDbUserName = ConfigurationManager.AppSettings["DefaultDbUserName"];

        public static readonly string GENERATE_STORED_PROC_PERMS = "Generate Stored Proc Perms";
        public static readonly string INCLUDE_DISABLED_CHECK = "Include Disabled Check";

        public eLanguage Language
        {
            get { return eLanguage.MsSql; }
        }
        public eCategory Category
        {
            get { return eCategory.Database; }
        }
        public IDictionary<string, string> Methods
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    { "Select Single By Id", "GenerateSelectSingleProc" },
                    { "Select Many By List", "GenerateSelectManyProc" },
                    { "Select Many By Criteria", "GenerateSelectManyByXProc" },
                    { "Select All", "GenerateSelectAllProc" },
                    { "Paginated Search", "GeneratePaginatedSearchProc" },
                    { "InsertProc", "GenerateInsertProc" },
                    { "UpdateProc", "GenerateUpdateProc" },
                    { "SetProc", "GenerateSetProc" },
                    { "Delete Single By Id", "GenerateDeleteSingleProc" },
                    { "Delete Many By List", "GenerateDeleteManyProc" },
                    { "Delete All", "GenerateDeleteAllProc" },
                };
            }
        }
        public IDictionary<string, bool> Options
        {
            get
            {
                return new Dictionary<string, bool>()
                {
                    { GENERATE_STORED_PROC_PERMS, false},
                    { INCLUDE_DISABLED_CHECK, false},
                };
            }
        }
        public override string TabType
        {
            get { return "SqlTabSize"; }
        }
        public int GetHash
        {
            get { return 42; }
        }

        public CodeGeneratorSql() { }

        public OutputObject GenerateSelectSingleProc(SqlTable sqlTable, GeneratorManifest manifest)
        {
            if (sqlTable == null)
                throw new ArgumentException("Sql table cannot be null");

            if (manifest == null)
                throw new ArgumentException("Manifest cannot be null");

            // todo: add in pk name list

            string procedure_name = NameFormatter.GenerateSqlStoredProcName(sqlTable.Name, eStoredProcType.SelectSingle, sqlTable.PkNames);
            string join_conjunction;

            var output = new OutputObject
            {
                Name = procedure_name + ".sql",
                Type = OutputObject.eObjectType.Sql
            };

            // sanity check - if there are no primary keys in the table, we cannot know 
            // what would a good choice would be for a qualifier. Abort and return error msg.
            if (sqlTable.PkList.Count == 0)
            {
                output.Body = $"/* Cannot generate {procedure_name}, no primary keys set on {sqlTable.Name}. */" + Environment.NewLine + Environment.NewLine;
                return output;
            }

            var sb = new StringBuilder();

            sb.AppendLine(GenerateSqlSpExistanceChecker(procedure_name, sqlTable.Schema));
            sb.AppendLine();
            sb.Append(GenerateDescriptionHeader(procedure_name, sqlTable.Schema));

            sb.AppendLine($"CREATE PROCEDURE [{sqlTable.Schema}].[{procedure_name}]");
            sb.AppendLine(GenerateSqlStoredProcParameters(sqlTable, eIncludedFields.PKOnly, false));

            sb.AppendLine("AS");
            sb.AppendLine();
            sb.AppendLine("SET NOCOUNT ON");
            sb.AppendLine();

            sb.AppendLine(GenerateSelectedColumns(sqlTable));

            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine($"FROM{AddTabs(1)}[{sqlTable.Schema}].[{sqlTable.Name}]");
            sb.AppendLine();

            #region Where Clause

            bool first_flag = true;
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

            if (manifest.GetOptionState(INCLUDE_DISABLED_CHECK) == GeneratorManifest.OptionsState.True)
                sb.AppendLine($"{join_conjunction}{NameFormatter.ToTSQLName(sqlTable.Name)}.[Disabled] = 0");

            #endregion

            sb.AppendLine("GO");
            sb.AppendLine();


            if (manifest.GetOptionState(GENERATE_STORED_PROC_PERMS) == GeneratorManifest.OptionsState.True)
            {
                sb.AppendLine(GenerateSqlStoredProcPerms(procedure_name));
                sb.AppendLine();
            }

            output.Body = sb.ToString();
            return output;
        }

        public OutputObject GenerateSelectManyProc(SqlTable sqlTable, List<string> sortFields, List<string> selectFields, bool generateStoredProcPerms, bool includeDisabledCheck)
        {
            if (sqlTable == null)
                throw new ArgumentException("Sql table cannot be null");

            if (sortFields == null || sortFields.Count == 0)
                throw new ArgumentException("Sort names cannot be null or empty");

            if (selectFields == null || selectFields.Count != 1)
                throw new ArgumentException("A select many proc can only be created if there is a single select column");

            string procedure_name = NameFormatter.GenerateSqlStoredProcName(sqlTable.Name, eStoredProcType.SelectMany, null);

            var search_column = sqlTable.Columns
                .Select(c => c.Value)
                .Where(c => c.Name == selectFields[0])
                .FirstOrDefault();

            var output = new OutputObject
            {
                Name = procedure_name + ".sql",
                Type = OutputObject.eObjectType.Sql
            };

            var sb = new StringBuilder();

            sb.AppendLine(GenerateSinglePkTableType(sqlTable, selectFields[0]));
            sb.AppendLine();

            // sample test:
            //declare @Ids[Spell].[TblSpellDataIdList]
            //insert @Ids(id) values(1)
            //insert @Ids(id) values(2)
            //EXEC[Spell].[SpellData_SelectMany] 0, 10, @Ids

            //CREATE PROCEDURE[Spell].[SpellData_SelectMany]
            //(
            //  @IdList[Spell].[TblSpellDataIdList] READONLY
            //)
            //AS

            //SET NOCOUNT ON

            //SELECT[SpellData].[Id],
            //		[SpellData].[Name],
            //		[SpellData].[Description],
            //		[SpellData].[Rarity],
            //		[SpellData].[AreaOfEffect],
            //		[SpellData].[Range],
            //		[SpellData].[CastingCost],
            //		[SpellData].[CastingDifficulty],
            //		[SpellData].[CastingTime],
            //		[SpellData].[Duration],
            //		[SpellData].[LearningDifficulty],
            //		[SpellData].[MagicResistancePenetration],
            //		[SpellData].[MagicSchool],
            //		[SpellData].[WrittenLength],
            //		[SpellData].[MaterialComponents],
            //		[SpellData].[VerbalComponents],
            //		[SpellData].[SomaticComponents],
            //		[SpellData].[PublishStateId]

            //        FROM[Spell].[SpellData]

            //        WHERE[SpellData].[Id]
            //        IN
            //     (
            //         SELECT[Id]
            //         FROM    @IdList
            //     )

            //ORDER BY[SpellData].[Name]
            //GO

            // now on to SP code
            sb.AppendLine(GenerateSqlSpExistanceChecker(procedure_name, sqlTable.Schema));
            sb.AppendLine();

            sb.AppendLine(GenerateDescriptionHeader(procedure_name, sqlTable.Schema));
            sb.AppendLine($"CREATE PROCEDURE [{sqlTable.Schema}].[{procedure_name}]");
            sb.AppendLine("(");
            sb.AppendLine(AddTabs(1) + $"@IdList [{sqlTable.Schema}].[Tbl{sqlTable.Name}IdList] READONLY");
            sb.AppendLine(")");

            sb.AppendLine("AS");
            sb.AppendLine();

            sb.AppendLine(GenerateSelectedColumns(sqlTable));

            sb.AppendLine();
            sb.AppendLine($"FROM{AddTabs(1)}[{sqlTable.Schema}].[{sqlTable.Name}]");
            sb.AppendLine();

            #region Where Clause

            sb.AppendLine($"WHERE{AddTabs(1)}[{search_column.Table.Name}].[{search_column.Name}] IN");
            sb.AppendLine("(");
            sb.AppendLine($"{AddTabs(1)}SELECT{AddTabs(1)}[{search_column.Table.Name}].[{search_column.Name}]");
            sb.AppendLine(AddTabs(1) + "FROM" + AddTabs(1) + "@IdList");
            sb.AppendLine(")");
            sb.AppendLine();

            if (includeDisabledCheck)
            {
                sb.AppendLine($"AND{AddTabs(1)}[{sqlTable.Name}].[Disabled] = 0");
                sb.AppendLine();
            }

            #endregion

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

        public OutputObject GenerateSelectManyByXProc(SqlTable sqlTable, List<string> sortFields, List<string> selectFields, bool generateStoredProcPerms, bool includeDisabledCheck)
        {
            if (sqlTable == null)
                throw new ArgumentException("Sql table cannot be null");

            if (sortFields == null || sortFields.Count == 0)
                throw new ArgumentException("Sort names cannot be null or empty");

            if (selectFields == null || selectFields.Count == 0)
                throw new ArgumentException("Select names cannot be null or empty");

            string procedure_name = NameFormatter.GenerateSqlStoredProcName(sqlTable.Name, eStoredProcType.SelectManyByX, selectFields);

            var output = new OutputObject
            {
                Name = procedure_name + ".sql",
                Type = OutputObject.eObjectType.Sql
            };

            var sb = new StringBuilder();

            sb.AppendLine(GenerateSqlSpExistanceChecker(procedure_name, sqlTable.Schema));
            sb.AppendLine();

            sb.AppendLine(GenerateDescriptionHeader(procedure_name, sqlTable.Schema));

            sb.AppendLine($"CREATE PROCEDURE [{sqlTable.Schema}].[{procedure_name}]");
            sb.AppendLine("(");
            sb.AppendLine(GenerateSelectClauseArguments(sqlTable, selectFields));
            sb.AppendLine(")");
            sb.AppendLine("AS");
            sb.AppendLine();
            sb.AppendLine("SET NOCOUNT ON");
            sb.AppendLine();

            sb.AppendLine(GenerateSelectedColumns(sqlTable));

            sb.AppendLine();
            sb.AppendLine($"FROM{AddTabs(1)}[{sqlTable.Schema}].[{sqlTable.Name}]");
            sb.AppendLine();

            if (includeDisabledCheck)
            {
                sb.AppendLine($"AND {NameFormatter.ToTSQLName(sqlTable.Name)}.[Disabled] = 0");
                sb.AppendLine();
            }

            sb.AppendLine(GenerateSelectClause(sqlTable, selectFields));
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

        public OutputObject GenerateSelectAllProc(SqlTable sqlTable, List<string> sortFields, bool generateStoredProcPerms, bool includeDisabledCheck)
        {
            if (sqlTable == null)
                throw new ArgumentException("Sql table cannot be null");

            if (sortFields == null || sortFields.Count == 0)
                throw new ArgumentException("Sort names cannot be null or empty");

            string procedure_name = NameFormatter.GenerateSqlStoredProcName(sqlTable.Name, eStoredProcType.SelectAll, null);

            var output = new OutputObject
            {
                Name = procedure_name + ".sql",
                Type = OutputObject.eObjectType.Sql
            };

            var sb = new StringBuilder();

            sb.AppendLine(GenerateSqlSpExistanceChecker(procedure_name, sqlTable.Schema));
            sb.AppendLine();

            sb.AppendLine(GenerateDescriptionHeader(procedure_name, sqlTable.Schema));

            sb.AppendLine($"CREATE PROCEDURE [{sqlTable.Schema}].[{procedure_name}]");
            sb.AppendLine("AS");
            sb.AppendLine();
            sb.AppendLine("SET NOCOUNT ON");
            sb.AppendLine();

            sb.AppendLine(GenerateSelectedColumns(sqlTable));

            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine($"FROM{AddTabs(1)}[{sqlTable.Schema}].[{sqlTable.Name}]");
            sb.AppendLine();

            if (includeDisabledCheck)
            {
                sb.AppendLine($"AND {NameFormatter.ToTSQLName(sqlTable.Name)}.[Disabled] = 0");
                sb.AppendLine();
            }

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

        public OutputObject GeneratePaginatedSearchProc(SqlTable sqlTable, List<string> sortFields, List<string> searchFields, bool generateStoredProcPerms, bool includeDisabledCheck)
        {
            if (sqlTable == null)
                throw new ArgumentException("Sql table cannot be null");

            if (sortFields == null || sortFields.Count == 0)
                throw new ArgumentException("Sort names cannot be null or empty");

            if (searchFields == null || searchFields.Count == 0)
                throw new ArgumentException("Search names cannot be null or empty");

            string procedure_name = NameFormatter.GenerateSqlStoredProcName(sqlTable.Name, eStoredProcType.SearchPaged, null);

            var output = new OutputObject
            {
                Name = procedure_name + ".sql",
                Type = OutputObject.eObjectType.Sql
            };

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

            sb.AppendLine(GenerateSqlSpExistanceChecker(procedure_name, sqlTable.Schema));
            sb.AppendLine(GenerateDescriptionHeader(procedure_name, sqlTable.Schema));

            sb.AppendLine($"CREATE PROCEDURE [{sqlTable.Schema}].[{procedure_name}]");
            sb.AppendLine("(");
            sb.AppendLine(AddTabs(1) + "@Skip INT,");
            sb.AppendLine(AddTabs(1) + "@Take INT,");
            sb.AppendLine(AddTabs(1) + "@SearchString VARCHAR(50),");
            sb.AppendLine(AddTabs(1) + "@Count INT OUTPUT");
            sb.AppendLine(")");

            sb.AppendLine("AS");
            sb.AppendLine();

            sb.AppendLine("SET NOCOUNT ON");
            sb.AppendLine();

            // 1) get total count
            sb.AppendLine("SELECT   @Count = COUNT(1)");
            sb.AppendLine($"FROM{AddTabs(1)}[{sqlTable.Schema}].[{sqlTable.Name}]");
            sb.AppendLine(GenerateSearchClause(sqlTable, searchFields, 0));

            if (includeDisabledCheck)
                sb.AppendLine($"AND {NameFormatter.ToTSQLName(sqlTable.Name)}.[Disabled] = 0");

            sb.AppendLine();

            // 2 get paginated results
            sb.AppendLine(GenerateSelectedColumns(sqlTable));

            sb.AppendLine();
            sb.AppendLine($"FROM{AddTabs(1)}[{sqlTable.Schema}].[{sqlTable.Name}]");
            sb.AppendLine();

            sb.AppendLine(GenerateSearchClause(sqlTable, searchFields, 0));

            if (includeDisabledCheck)
                sb.AppendLine($"AND {NameFormatter.ToTSQLName(sqlTable.Name)}.[Disabled] = 0");

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

        public OutputObject GenerateInsertProc(SqlTable sqlTable, bool generateStoredProcPerms)
        {
            if (sqlTable == null)
                throw new ArgumentException("Sql table cannot be null");

            string procedure_name = NameFormatter.GenerateSqlStoredProcName(sqlTable.Name, eStoredProcType.Insert, null);

            var output = new OutputObject
            {
                Name = procedure_name + ".sql",
                Type = OutputObject.eObjectType.Sql
            };

            var sb = new StringBuilder();

            sb.AppendLine(GenerateSqlSpExistanceChecker(procedure_name, sqlTable.Schema));
            sb.AppendLine();

            sb.AppendLine(GenerateDescriptionHeader(procedure_name, sqlTable.Schema));

            sb.AppendLine($"CREATE PROCEDURE [{sqlTable.Schema}].[{procedure_name}]");
            sb.AppendLine(GenerateSqlStoredProcParameters(sqlTable, eIncludedFields.NoIdentities, false));

            sb.AppendLine("AS");
            sb.AppendLine();
            sb.AppendLine("SET NOCOUNT ON");
            sb.AppendLine();

            sb.AppendLine($"INSERT [{sqlTable.Schema}].[{sqlTable.Name}]");
            sb.Append("(");

            // list selected columns
            bool first_flag = true;

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if (!sql_column.IsIdentity)
                {
                    if (first_flag)
                        first_flag = false;
                    else
                        sb.Append(",");

                    sb.Append(Environment.NewLine + AddTabs(1) + NameFormatter.ToTSQLName(sql_column.Name));
                }
            }

            sb.Append(Environment.NewLine + ")");
            sb.Append(Environment.NewLine + "VALUES");
            sb.Append(Environment.NewLine + "(");

            // Build where clause
            first_flag = true;

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if (!sql_column.IsIdentity)
                {
                    if (first_flag)
                        first_flag = false;
                    else
                        sb.Append(",");

                    sb.Append(Environment.NewLine + AddTabs(1) + NameFormatter.ToTSQLVariableName(sql_column));
                }
            }

            sb.AppendLine(Environment.NewLine + ")");
            sb.AppendLine();

            // todo: hack - fix this, Id should not be hard coded
            sb.AppendLine("SET @Id = SCOPE_IDENTITY()");
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

        public OutputObject GenerateUpdateProc(SqlTable sqlTable, bool generateStoredProcPerms)
        {
            if (sqlTable == null)
                throw new ArgumentException("Sql table cannot be null");

            string procedure_name = NameFormatter.GenerateSqlStoredProcName(sqlTable.Name, eStoredProcType.Update, null);

            var output = new OutputObject
            {
                Name = procedure_name + ".sql",
                Type = OutputObject.eObjectType.Sql
            };

            // sanity check - if there are no primary keys in the table, we cannot know 
            // what would a good choice would be for a qualifier. Abort and return error msg.
            if (sqlTable.PkList.Count == 0)
            {
                output.Body = $"/* Cannot generate {procedure_name}, no primary keys set on {sqlTable.Name}. */" + Environment.NewLine + Environment.NewLine;
                return output;
            }

            var sb = new StringBuilder();

            sb.AppendLine(GenerateSqlSpExistanceChecker(procedure_name, sqlTable.Schema));
            sb.AppendLine();

            sb.AppendLine(GenerateDescriptionHeader(procedure_name, sqlTable.Schema));

            sb.AppendLine($"CREATE PROCEDURE [{sqlTable.Schema}].[{procedure_name}]");
            sb.AppendLine(GenerateSqlStoredProcParameters(sqlTable, eIncludedFields.All, true));

            sb.AppendLine("AS");
            sb.AppendLine();

            sb.AppendLine($"UPDATE{AddTabs(1)}[{sqlTable.Schema}].[{sqlTable.Name}]");
            sb.AppendLine();

            // list selected columns
            bool first_flag = true;

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if (!sql_column.IsPk && !sql_column.IsIdentity)
                {
                    if (first_flag)
                    {
                        sb.Append("SET");
                        first_flag = false;
                    }
                    else
                    {
                        sb.Append("," + Environment.NewLine);
                    }

                    sb.Append($"{AddTabs(2)}[{sql_column.Name}] = {NameFormatter.ToTSQLVariableName(sql_column)}");
                }
            }

            sb.AppendLine();
            sb.AppendLine();

            // Build where clause
            first_flag = true;

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if (sql_column.IsPk)
                {
                    if (first_flag)
                    {
                        sb.Append("WHERE" + AddTabs(1));
                        first_flag = false;
                    }
                    else
                    {
                        sb.Append(Environment.NewLine + "AND" + AddTabs(1));
                    }

                    sb.Append($"[{sql_column.Name}] = {NameFormatter.ToTSQLVariableName(sql_column)}");
                }
            }

            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("SET @Count = @@ROWCOUNT");
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

        public OutputObject GenerateSetProc(SqlTable sqlTable, bool generateStoredProcPerms)
        {
            if (sqlTable == null)
                throw new ArgumentException("Sql table cannot be null");

            string procedure_name = NameFormatter.GenerateSqlStoredProcName(sqlTable.Name, eStoredProcType.UpdateInsert, null);

            var output = new OutputObject
            {
                Name = procedure_name + ".sql",
                Type = OutputObject.eObjectType.Sql
            };

            // sanity check - if there are no primary keys in the table, we cannot know 
            // what would a good choice would be for a qualifier. Abort and return error msg.
            if (sqlTable.PkList.Count == 0)
            {
                output.Body = $"/* Cannot generate {procedure_name}, no primary keys set on {sqlTable.Name}. */" + Environment.NewLine + Environment.NewLine;
                return output;
            }

            var sb = new StringBuilder();

            sb.AppendLine(GenerateSqlSpExistanceChecker(procedure_name, sqlTable.Schema));
            sb.AppendLine();

            sb.AppendLine(GenerateDescriptionHeader(procedure_name, sqlTable.Schema));

            sb.AppendLine($"CREATE PROCEDURE [{sqlTable.Schema}].[{procedure_name}]");
            sb.AppendLine(GenerateSqlStoredProcParameters(sqlTable, eIncludedFields.All, true));

            sb.AppendLine("AS");
            sb.AppendLine();
            sb.AppendLine("SET NOCOUNT ON");
            sb.AppendLine();

            #region Existance Check
            sb.Append($"IF EXISTS (SELECT 1 FROM [{sqlTable.Schema}].[{sqlTable.Name}]");

            // Build where clause
            bool first_flag = true;

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if (sql_column.IsPk)
                {
                    if (first_flag)
                    {
                        sb.Append(" WHERE ");
                        first_flag = false;
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
            first_flag = true;

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if (!sql_column.IsPk && !sql_column.IsIdentity)
                {
                    if (first_flag)
                    {
                        sb.Append(AddTabs(1) + "SET" + AddTabs(2));
                        first_flag = false;
                    }
                    else
                    {
                        sb.Append("," + Environment.NewLine + AddTabs(3));
                    }

                    sb.Append($"[{sql_column.Table.Name}].[{sql_column.Name}] = {NameFormatter.ToTSQLVariableName(sql_column)}");
                }
            }

            sb.Append(Environment.NewLine);

            // Build where clause
            first_flag = true;

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if (sql_column.IsPk)
                {
                    if (first_flag)
                    {
                        sb.Append(AddTabs(1) + "WHERE" + AddTabs(1));
                        first_flag = false;
                    }
                    else
                    {
                        sb.Append(Environment.NewLine + AddTabs(1) + "AND" + AddTabs(1));
                    }

                    sb.Append($"[{sql_column.Table.Name}].[{sql_column.Name}] = {NameFormatter.ToTSQLVariableName(sql_column)}");
                }
            }

            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine(AddTabs(1) + "SET @Count = @@ROWCOUNT");
            #endregion

            sb.AppendLine("END");
            sb.AppendLine("ELSE");
            sb.AppendLine("BEGIN");

            #region Insert Clause
            sb.AppendLine(AddTabs(1) + "-- New Entry, Insert");
            sb.AppendLine(AddTabs(1) + "INSERT [" + sqlTable.Name + "]");
            sb.AppendLine(AddTabs(1) + "(");

            // list selected columns
            first_flag = true;

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if (!sql_column.IsIdentity)
                {
                    if (first_flag)
                    {
                        sb.Append(AddTabs(2));
                        first_flag = false;
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
            first_flag = true;

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if (!sql_column.IsIdentity)
                {
                    if (first_flag)
                    {
                        sb.Append(Environment.NewLine + AddTabs(2));
                        first_flag = false;
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
            sb.AppendLine(AddTabs(1) + "SET @Count = @@SCOPE_IDENTITY()");
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

        public OutputObject GenerateDeleteSingleProc(SqlTable sqlTable, bool generateStoredProcPerms)
        {
            if (sqlTable == null)
                throw new ArgumentException("Sql table cannot be null");

            string procedure_name = NameFormatter.GenerateSqlStoredProcName(sqlTable.Name, eStoredProcType.DelSingle, null);

            var output = new OutputObject
            {
                Name = procedure_name + ".sql",
                Type = OutputObject.eObjectType.Sql
            };

            // sanity check - if there are no primary keys in the table, we cannot know 
            // what would a good choice would be for a qualifier. Abort and return error msg.
            if (sqlTable.PkList.Count == 0)
            {
                output.Body = $"/* Cannot generate {procedure_name}, no primary keys set on {sqlTable.Name}. */" + Environment.NewLine + Environment.NewLine;
                return output;
            }

            var sb = new StringBuilder();

            sb.AppendLine(GenerateSqlSpExistanceChecker(procedure_name, sqlTable.Schema));
            sb.AppendLine();

            sb.AppendLine(GenerateDescriptionHeader(procedure_name, sqlTable.Schema));

            sb.AppendLine($"CREATE PROCEDURE [{sqlTable.Schema}].[{procedure_name}]");
            sb.AppendLine(GenerateSqlStoredProcParameters(sqlTable, eIncludedFields.PKOnly, true));

            sb.AppendLine("AS");
            sb.AppendLine();
            sb.AppendLine("SET NOCOUNT ON");
            sb.AppendLine();
            sb.AppendLine($"DELETE{AddTabs(1)}[{sqlTable.Schema}].[{sqlTable.Name}]");

            #region Where Clause
            bool first_flag = true;

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if (sql_column.IsPk)
                {
                    if (first_flag)
                    {
                        sb.Append("WHERE" + AddTabs(1));
                        first_flag = false;
                    }
                    else
                    {
                        sb.Append(Environment.NewLine + "AND" + AddTabs(2));
                    }

                    sb.Append($"[{sql_column.Table.Name}].[{sql_column.Name}] = {NameFormatter.ToTSQLVariableName(sql_column)}");
                }
            }
            #endregion

            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("SET @Count = @@ROWCOUNT");
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

        public OutputObject GenerateDeleteManyProc(SqlTable sqlTable, List<string> selectFields, bool generateStoredProcPerms)
        {
            if (sqlTable == null)
                throw new ArgumentException("Sql table cannot be null");

            if (selectFields == null || selectFields.Count != 1)
                throw new ArgumentException("A select many proc can only be created if there is a single select column");

            string procedure_name = NameFormatter.GenerateSqlStoredProcName(sqlTable.Name, eStoredProcType.DelMany, null);

            var search_column = sqlTable.Columns
                .Select(c => c.Value)
                .Where(c => c.Name == selectFields[0])
                .FirstOrDefault();

            var output = new OutputObject
            {
                Name = procedure_name + ".sql",
                Type = OutputObject.eObjectType.Sql
            };

            var sb = new StringBuilder();

            sb.AppendLine(GenerateSinglePkTableType(sqlTable, selectFields[0]));
            sb.AppendLine();

            // sample test:
            //declare @Ids[Spell].[TblSpellDataIdList]
            //insert @Ids(id) values(1)
            //insert @Ids(id) values(2)
            //EXEC[Spell].[SpellData_DeleteMany] 0, 10, @Ids

            //CREATE PROCEDURE[Spell].[SpellData_DeleteMany]
            //(
            //  @IdList[Spell].[TblSpellDataIdList] READONLY
            //)
            //AS

            //DELETE[Spell].[SpellData]
            //WHERE[SpellData].[Id] IN
            //     (
            //         SELECT[Id]
            //         FROM    @IdList
            //     )
            //GO

            // now on to SP code
            sb.AppendLine(GenerateSqlSpExistanceChecker(procedure_name, sqlTable.Schema));
            sb.AppendLine();

            sb.AppendLine(GenerateDescriptionHeader(procedure_name, sqlTable.Schema));
            sb.AppendLine($"CREATE PROCEDURE [{sqlTable.Schema}].[{procedure_name}]");
            sb.AppendLine("(");
            sb.AppendLine(AddTabs(1) + $"@IdList [{sqlTable.Schema}].[Tbl{sqlTable.Name}IdList] READONLY");
            sb.AppendLine(")");

            sb.AppendLine("AS");
            sb.AppendLine();

            sb.AppendLine($"DELETE{AddTabs(1)}[{sqlTable.Schema}].[{sqlTable.Name}]");
            sb.AppendLine();

            #region Where Clause

            sb.AppendLine($"WHERE{AddTabs(1)}[{search_column.Table.Name}].[{search_column.Name}] IN");
            sb.AppendLine("(");
            sb.AppendLine(AddTabs(1) + "SELECT " + AddTabs(1) + NameFormatter.ToTSQLName(search_column.Name));
            sb.AppendLine(AddTabs(1) + "FROM " + AddTabs(1) + "@IdList");
            sb.AppendLine(")");
            sb.AppendLine();

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

        public OutputObject GenerateDeleteAllProc(SqlTable sqlTable, bool generateStoredProcPerms)
        {
            if (sqlTable == null)
                throw new ArgumentException("Sql table cannot be null");

            string procedure_name = NameFormatter.GenerateSqlStoredProcName(sqlTable.Name, eStoredProcType.DelAll, null);

            var output = new OutputObject
            {
                Name = procedure_name + ".sql",
                Type = OutputObject.eObjectType.Sql
            };

            var sb = new StringBuilder();

            sb.AppendLine(GenerateSqlSpExistanceChecker(procedure_name, sqlTable.Schema));
            sb.AppendLine();

            sb.AppendLine(GenerateDescriptionHeader(procedure_name, sqlTable.Schema));

            sb.AppendLine($"CREATE PROCEDURE [{sqlTable.Schema}].[{procedure_name}]");
            sb.AppendLine(GenerateSqlStoredProcParameters(sqlTable, eIncludedFields.None, true));

            sb.AppendLine("AS");
            sb.AppendLine();

            //sb.AppendLine("SET NOCOUNT ON");
            //sb.AppendLine();

            sb.AppendLine($"DELETE{AddTabs(1)}[{sqlTable.Schema}].[{sqlTable.Name}]");
            sb.AppendLine();

            sb.AppendLine($"SET{AddTabs(2)}@Count = @@ROWCOUNT");
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

        public string GenerateGetTableDetailsStoredProcedure()
        {
            var procedure_name = "sp_TableDetails";
            var schema = "dbo";

            var sb = new StringBuilder();

            sb.AppendLine("USE MASTER");
            sb.AppendLine("GO");
            sb.AppendLine();

            sb.AppendLine(GenerateSqlSpExistanceChecker(procedure_name, schema));

            sb.AppendLine("/*");
            sb.AppendLine(AddTabs(1) + $"[{schema}].[{procedure_name}]");
            sb.AppendLine(AddTabs(1) + "By Roger Hill");
            sb.AppendLine();

            sb.AppendLine(AddTabs(1) + "Collates table column data");
            sb.AppendLine(AddTabs(1) + "into single table for easy use");
            sb.AppendLine("*/");
            sb.AppendLine();

            sb.AppendLine($"CREATE PROCEDURE [{schema}].[{procedure_name}]");
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

        public string GenerateGetTableDetailsLogic()
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

        public string GenerateSelectedColumns(SqlTable sqlTable)
        {
            if (sqlTable == null)
                throw new ArgumentException("Sql table cannot be null");

            var sb = new StringBuilder();

            bool first_flag = true;
            sb.Append("SELECT" + AddTabs(1));

            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if (first_flag)
                    first_flag = false;
                else
                    sb.Append("," + Environment.NewLine + AddTabs(2));

                sb.Append(NameFormatter.ToTSQLName(sql_column.Table.Name) + "." + NameFormatter.ToTSQLName(sql_column.Name));
            }

            return sb.ToString();
        }

        // Script Header Generation
        public string GenerateSqlScriptHeader(string databaseName)
        {
            if (string.IsNullOrEmpty(databaseName))
                throw new Exception("Cannot generate stored procedure name without a database name.");

            var sb = new StringBuilder();

            // Script file header
            sb.Append("/* " + GenerateAuthorNotice() + " */");
            sb.Append(Environment.NewLine);
            sb.Append("USE [" + databaseName + "]");
            sb.Append(Environment.NewLine + "GO");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);

            return sb.ToString();
        }

        public string GenerateSqlStoredProcPerms(string storedProcedureName)
        {
            if (string.IsNullOrEmpty(storedProcedureName))
                throw new Exception("Cannot generate permissions without a stored procedure name.");

            var sb = new StringBuilder();

            sb.AppendLine("GRANT EXECUTE ON [dbo].[" + storedProcedureName + "] TO [" + _DefaultDbUserName + "]");
            sb.AppendLine("GO");

            return sb.ToString();
        }

        // helper script methods

        private string GenerateSinglePkTableType(SqlTable sqlTable, string columnName)
        {
            if (sqlTable == null)
                throw new ArgumentException("Sql table cannot be null");

            if (string.IsNullOrWhiteSpace(columnName))
                throw new ArgumentException("Column name cannot be null or empty");

            // this will generate a sql table type based off the primary keys from a sql table.

            // EX:
            // IF EXISTS(SELECT name FROM sys.table_types WHERE name = 'TblIdList' AND is_table_type = 1)
            // DROP TYPE[dbo].[TblIdList]
            // GO

            // CREATE TYPE[dbo].[TblIdList] AS TABLE
            // (
            //      [Id] [int] NULL
            // )
            // GO

            var sb = new StringBuilder();
            bool first_flag = true;

            sb.AppendLine($"IF EXISTS (SELECT name FROM sys.table_types WHERE name = 'Tbl{sqlTable.Name}IdList' AND is_table_type = 1)");
            sb.AppendLine($"DROP TYPE [{sqlTable.Schema}].[Tbl{sqlTable.Name}IdList]");
            sb.AppendLine("GO");
            sb.AppendLine();

            sb.AppendLine($"CREATE TYPE [{sqlTable.Schema}].[Tbl{sqlTable.Name}IdList] AS TABLE");
            sb.AppendLine("(");

            foreach (var column in sqlTable.Columns.Values)
            {
                if (column.Name == columnName)
                {
                    if (first_flag)
                        first_flag = false;
                    else
                        sb.Append("," + Environment.NewLine);

                    sb.Append(AddTabs(1) + NameFormatter.SQLTypeToColumnDefinition(column));
                }
            }

            sb.AppendLine(Environment.NewLine + ")");
            sb.Append("GO");

            return sb.ToString();
        }

        private string GeneratePkTableType(SqlTable sqlTable)
        {
            if (sqlTable == null)
                throw new ArgumentException("Sql table cannot be null");

            // this will generate a sql table type based off the primary keys from a sql table.

            // EX:
            // IF EXISTS(SELECT name FROM sys.table_types WHERE name = 'TblIdList' AND is_table_type = 1)
            // DROP TYPE[dbo].[TblIdList]
            // GO

            // CREATE TYPE[dbo].[TblIdList] AS TABLE
            // (
            //      [Id] [int] NULL
            // )
            // GO

            var sb = new StringBuilder();
            bool first_flag = true;

            sb.AppendLine($"IF EXISTS (SELECT name FROM sys.table_types WHERE name = 'Tbl{sqlTable.Name}IdList' AND is_table_type = 1)");
            sb.AppendLine($"DROP TYPE [{sqlTable.Schema}].[Tbl{sqlTable.Name}IdList]");
            sb.AppendLine("GO");
            sb.AppendLine();

            sb.AppendLine($"CREATE TYPE [{sqlTable.Schema}].[Tbl{sqlTable.Name}IdList] AS TABLE");
            sb.AppendLine("(");

            foreach (var column in sqlTable.Columns.Values)
            {
                if (column.IsPk)
                {
                    if (first_flag)
                        first_flag = false;
                    else
                        sb.Append("," + Environment.NewLine);

                    sb.Append(AddTabs(1) + NameFormatter.SQLTypeToColumnDefinition(column));
                }
            }

            sb.AppendLine(Environment.NewLine + ")");
            sb.Append("GO");

            return sb.ToString();
        }

        private string GenerateSqlSpExistanceChecker(string procedureName, string schema)
        {
            if (string.IsNullOrEmpty(procedureName))
                throw new Exception("Cannot generate sql existance check without a procedure name");

            if (string.IsNullOrEmpty(schema))
                throw new Exception("Cannot generate sql existance check without schema");

            var sb = new StringBuilder();

            sb.AppendLine($"IF EXISTS (SELECT name FROM sys.objects WHERE name = '{procedureName}' AND SCHEMA_NAME(schema_id) = '{schema}' AND type = 'P')");
            sb.AppendLine($"DROP PROCEDURE [{schema}].[{procedureName}]");
            sb.Append("GO");

            return sb.ToString();
        }

        /// <summary>
        /// Generates an inline sql warning message
        /// </summary>
        private string GenerateSqlWarning(string message)
        {
            if (string.IsNullOrEmpty(message))
                throw new ArgumentException("Cannot warning with empty message");

            var sb = new StringBuilder();

            sb.AppendLine("/*");
            sb.AppendLine(message);
            sb.Append("*/");

            return sb.ToString();
        }

        private string GenerateDescriptionHeader(string procedureName, string schema)
        {
            if (string.IsNullOrEmpty(procedureName))
                throw new ArgumentException("Cannot generate procedure header without a procedure name");

            if (string.IsNullOrEmpty(schema))
                throw new ArgumentException("Cannot generate procedure header without a schema");

            var sb = new StringBuilder();

            sb.AppendLine("/*");
            sb.AppendLine($"{AddTabs(1)}{procedureName}");
            sb.AppendLine($"{AddTabs(1)}EXEC [{schema}].[{procedureName}] -- todo set good test values");
            sb.AppendLine(AddTabs(1) + GenerateAuthorNotice());
            sb.Append("*/");

            return sb.ToString();
        }

        private string GenerateSqlStoredProcParameters(SqlTable sqlTable, eIncludedFields includedFields, bool includeCountParameter)
        {
            if (sqlTable == null)
                throw new ArgumentException("Sql table cannot be null");

            #region Sample Output
            //(
            //    @CountryID INT,
            //    @CountryName VARCHAR(255)
            //)
            #endregion

            var sb = new StringBuilder();
            bool first_flag = true;

            sb.Append("(");

            // list query parameters
            foreach (var sql_column in sqlTable.Columns.Values)
            {
                if ((includedFields == eIncludedFields.All) ||
                    (includedFields == eIncludedFields.PKOnly && sql_column.IsPk) ||
                    (includedFields == eIncludedFields.NoIdentities))
                {
                    if (first_flag)
                        first_flag = false;
                    else
                        sb.Append(",");

                    sb.Append(Environment.NewLine + AddTabs(1) + NameFormatter.ToTSQLVariableName(sql_column) + " " + NameFormatter.ToTSQLType(sql_column));

                    if (includedFields == eIncludedFields.NoIdentities && sql_column.IsIdentity)
                        sb.Append(" OUTPUT");
                }
            }

            // todo: this could generate invalid syntax if there is a column named 'Count' in the table already...
            if (includeCountParameter)
            {
                if (!first_flag)
                    sb.Append(",");

                sb.Append(Environment.NewLine + AddTabs(1) + "@Count INT OUTPUT");
            }

            sb.Append(Environment.NewLine + ")");

            return sb.ToString();
        }

        private string GenerateOrderByClause(SqlTable sqlTable, List<string> columnNames)
        {
            if (sqlTable == null)
                throw new ArgumentException("Sql table cannot be null");

            if (columnNames == null || columnNames.Count == 0)
                throw new ArgumentException("Column names cannot be null or empty");

            if (columnNames == null || columnNames.Count == 0)
                return "-- ORDER BY [<A_COLUMN_NAME>]";

            var sb = new StringBuilder();

            sb.Append("ORDER" + AddTabs(1) + "BY ");

            bool flag_first_value = true;

            foreach (var item in columnNames)
            {
                if (flag_first_value)
                    flag_first_value = false;
                else
                    sb.Append(",");

                sb.Append(NameFormatter.ToTSQLName(sqlTable.Name) + "." + NameFormatter.ToTSQLName(item));
            }

            return sb.ToString();
        }

        private string GenerateSearchClause(SqlTable sqlTable, List<string> columnNames, int tabsIn)
        {
            if (sqlTable == null)
                throw new ArgumentException("Sql table cannot be null");

            if (columnNames == null || columnNames.Count == 0)
                throw new ArgumentException("Column names cannot be null or empty");

            if (tabsIn < 0)
                throw new ArgumentException("Tabs in cannot be less than 0");

            var sb = new StringBuilder();

            bool flag_first_value = true;

            sb.Append(AddTabs(tabsIn) + "WHERE" + AddTabs(1));

            if (columnNames.Count > 1)
                sb.Append(Environment.NewLine + AddTabs(tabsIn) + "(" + Environment.NewLine + AddTabs(tabsIn + 1));

            foreach (var item in columnNames)
            {
                if (flag_first_value)
                    flag_first_value = false;
                else
                    sb.Append(Environment.NewLine + AddTabs(tabsIn + 1) + "OR ");

                sb.Append($"[{sqlTable.Name}].[{item}] LIKE '%' + @SearchString + '%'");
            }

            if (columnNames.Count > 1)
                sb.Append(Environment.NewLine + AddTabs(tabsIn) + ")");

            return sb.ToString();
        }

        private string GenerateSelectClause(SqlTable sqlTable, List<string> columnNames)
        {
            if (sqlTable == null)
                throw new ArgumentException("Sql table cannot be null");

            if (columnNames == null || columnNames.Count == 0)
                throw new ArgumentException("Column names cannot be null or empty");

            #region Sample output:
            // WHERE [UserId]    = @UserId
            // AND [Age]        = @Age
            // AND [ShoeSize]   = @ShoeSize
            #endregion

            if (columnNames == null || columnNames.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();

            bool first_flag = true;
            string join_clause = "WHERE ";

            foreach (var item in columnNames)
            {
                string field_name = NameFormatter.ToTSQLName(sqlTable.Name) + "." + NameFormatter.ToTSQLName(item);
                sb.Append(join_clause + AddTabs(1) + field_name + " = " + NameFormatter.ToTSQLVariableName(sqlTable.Columns[item]));

                if (first_flag)
                {
                    first_flag = false;
                    join_clause = Environment.NewLine + "AND ";
                }
            }

            return sb.ToString();
        }

        private string GenerateSelectClauseArguments(SqlTable sqlTable, List<string> columnNames)
        {
            if (sqlTable == null)
                throw new ArgumentException("Sql table cannot be null");

            if (columnNames == null || columnNames.Count == 0)
                throw new ArgumentException("Column names cannot be null or empty");

            if (columnNames == null)
                return AddTabs(1) + "-- @A_VALUE [SOME_DATA_TYPE]";

            var sb = new StringBuilder();

            bool flag_first_value = true;

            foreach (var item in columnNames)
            {
                if (flag_first_value)
                    flag_first_value = false;
                else
                    sb.Append("," + Environment.NewLine);

                sb.Append(AddTabs(1) + NameFormatter.ToTSQLVariableName(sqlTable.Columns[item]) + AddTabs(2) + NameFormatter.ToTSQLType(sqlTable.Columns[item]));
            }

            return sb.ToString();
        }

        //private static string GenerateSqlListCode()
        //{
        //    // This is the code that takes a single sql text field and 
        //    // breaks it up to use as a list of int varables.

        //    var sb = new StringBuilder();

        //    sb.AppendLine("DECLARE @IDListPosition INT");
        //    sb.AppendLine("DECLARE @ArrValue VARCHAR(MAX)");
        //    sb.AppendLine();
        //    sb.AppendLine("DECLARE @TableVar TABLE");
        //    sb.AppendLine("(");
        //    sb.AppendLine(AddTabs(1) + "IdValue VARCHAR(50) NOT NULL");
        //    sb.AppendLine(")");
        //    sb.AppendLine();
        //    sb.AppendLine("SET @IDList = COALESCE(@IDList ,'')");
        //    sb.AppendLine();
        //    sb.AppendLine("IF @IDList <> ''");
        //    sb.AppendLine("BEGIN");
        //    sb.AppendLine();
        //    sb.AppendLine(AddTabs(1) + "SET @IDList = @IDList + ','");
        //    sb.AppendLine(AddTabs(1) + "WHILE PATINDEX('%,%', @IDList) <> 0");
        //    sb.AppendLine();
        //    sb.AppendLine(AddTabs(1) + "BEGIN");
        //    sb.AppendLine();
        //    sb.AppendLine(AddTabs(2) + "SELECT @IDListPosition = PATINDEX('%,%', @IDList)");
        //    sb.AppendLine(AddTabs(2) + "SELECT @ArrValue = left(@IDList, @IDListPosition - 1)");
        //    sb.AppendLine();
        //    sb.AppendLine(AddTabs(2) + "INSERT INTO @TableVar (IdValue) VALUES (@ArrValue)");
        //    sb.AppendLine();
        //    sb.AppendLine(AddTabs(2) + "SELECT @IDList = stuff(@IDList, 1, @IDListPosition, '')");
        //    sb.AppendLine();
        //    sb.AppendLine(AddTabs(1) + "END");
        //    sb.AppendLine("END");

        //    return sb.ToString();
        //}
    }
}