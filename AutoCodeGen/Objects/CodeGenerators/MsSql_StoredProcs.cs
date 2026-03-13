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

using DAL.Net.SqlMetadata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace AutoCodeGen
{
    public class MsSql_StoredProcs : IGenerator
    {
        private const string OutputPath = "MsSql\\";
        private const string FEATURE_INSERT = "Insert table SP";

        // option names
        private const string EXISTENCE_CHECKS = "Include existence checks";

        private static HashSet<char> _UndesirableChars = new()
        {
            '!', '%', '^', '*', '(', ')', '+', '\\', '=',
            '{', '}', '[', ']', ':', ';', '|', '\'', '<', '>', ',',
            '?', '/', '~', '`', '#', '"', '\t', '&',
            '.', '-', ' ', '@' // <-- allowed in SQL object names but not in variable and parameter names.
        };

        public string Language => "SQL";
        public string Category => "Database";
        public string Name => "MS-SQL Stored Procs";
        public string Description => "Generates MS-SQL stored procedures.";
        public string[] FeatureNames => [FEATURE_INSERT];

        public Dictionary<string, string> DefaultOptions => new()
        {
            { EXISTENCE_CHECKS, "true" },
        };

        public OutputObject Process(string feature, SqlTable sqlTable, Dictionary<string, string> options)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(feature, nameof(feature));
            ArgumentNullException.ThrowIfNull(sqlTable, nameof(sqlTable));
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            return feature switch
            {
                FEATURE_INSERT => GenerateInsertSingleMethod(sqlTable, options),

                _ => throw new ArgumentOutOfRangeException($"Mode {feature} is not supported by {Name} generator."),
            };
        }

        public OutputObject GenerateInsertSingleMethod(SqlTable sqlTable, Dictionary<string, string> options)
        {
            ArgumentNullException.ThrowIfNull(sqlTable);
            ArgumentNullException.ThrowIfNull(options);

            string spName = GenerateSqlStoredProcName(sqlTable, eStoredProcType.InsertSingle);
            var sb = new StringBuilder();

            sb.AppendLine(GenerateStoredProcComments(sqlTable, eStoredProcType.InsertSingle));

            sb.AppendLine($"CREATE PROCEDURE {spName}");
            sb.AppendLine(GenerateSqlStoredProcParameters(sqlTable, eIncludedFields.All));

            sb.AppendLine("AS");
            sb.AppendLine();

            sb.AppendLine($"INSERT [{sqlTable.Schema}].[{sqlTable.Name}]");
            sb.Append("(");

            // list selected columns
            bool firstFlag = true;

            foreach (var col in sqlTable.ColumnList)
            {
                if (col.IsIdentity)
                    continue;

                if (firstFlag)
                    firstFlag = false;
                else
                    sb.Append(",");

                sb.Append(Environment.NewLine + Formatter.AddTabs(1) + ToTSQLColumnName(col));

            }

            sb.Append(Environment.NewLine + ")");
            sb.Append(Environment.NewLine + "VALUES");
            sb.Append(Environment.NewLine + "(");

            // Build where clause
            firstFlag = true;

            foreach (var col in sqlTable.ColumnList)
            {
                if (col.IsIdentity)
                    continue;

                if (firstFlag)
                    firstFlag = false;
                else
                    sb.Append(",");

                sb.Append(Environment.NewLine + Formatter.AddTabs(1) + ToTSQLVariableName(col));
            }

            sb.AppendLine(Environment.NewLine + ")");
            sb.AppendLine("GO");
            sb.AppendLine();

            return new OutputObject
            {
                FileName = $"{spName}.sql",
                Body = sb.ToString(),
                OutputPath = $"{OutputPath}\\stored_procs",
            };
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// Helper Methods

        /// <summary>
        /// Returns a T-SQL representation of the SQL datatype.
        /// 
        /// Sample: VARCHAR(50)
        /// </summary>
        private static string ToTSQLType(SqlColumn sqlColumn)
        {
            SqlDbType sqlType = sqlColumn.SqlDataType;

            if (sqlType == SqlDbType.Binary || sqlType == SqlDbType.Char || sqlType == SqlDbType.NChar || sqlType == SqlDbType.NVarChar ||
                sqlType == SqlDbType.VarBinary || sqlType == SqlDbType.VarChar)
            {
                if (sqlColumn.Length == -1)
                {
                    return $"{sqlColumn.SqlDataType.ToString().ToUpper()}(MAX)";
                }
                else
                {
                    return $"{sqlColumn.SqlDataType.ToString().ToUpper()}({sqlColumn.Length})";
                }
            }
            else if (sqlType == SqlDbType.Decimal)
            {
                return $"{sqlColumn.SqlDataType.ToString().ToUpper()}({sqlColumn.Precision},{sqlColumn.Scale})";
            }
            else
            {
                return sqlColumn.SqlDataType.ToString().ToUpper();
            }
        }

        /// <summary>
        /// Returns the SQL column name formatted for use in a T-SQL script.
        /// 
        /// Sample: Foo Bar -> [Foo Bar]
        /// </summary>
        private static string ToTSQLName(string input)
        {
            return $"[{input}]";
        }

        /// <summary>
        /// Returns a string containing a column definition for a table creation script.
        /// 
        /// ex: [Id] [int] NULL
        /// </summary>
        private static string SQLTypeToColumnDefinition(SqlColumn sqlColumn)
        {
            string nullable;

            if (sqlColumn.IsNullable)
                nullable = "NULL";
            else
                nullable = "NOT NULL";

            return $"{ToTSQLName(sqlColumn.Name)} {ToTSQLType(sqlColumn)} {nullable}";
        }

        /// <summary>
        /// Sample: [dbo].[Events_SelectSingle]
        /// </summary>
        private static string GenerateSqlStoredProcName(SqlTable table, eStoredProcType procType, IEnumerable<string> selectedFields = null)
        {
            ArgumentNullException.ThrowIfNull(table);

            string suffix;
            string selectedFieldsString = (selectedFields == null) ? string.Empty : string.Join(string.Empty, selectedFields);

            switch (procType)
            {
                case eStoredProcType.InsertSingle: suffix = "InsertSingle"; break;
                case eStoredProcType.InsertMultiple: suffix = "InsertMultiple"; break;

                case eStoredProcType.SelectSingle: suffix = "SelectSingle"; break;
                case eStoredProcType.SelectMany: suffix = "SelectMany"; break;
                case eStoredProcType.SelectManyByX: suffix = $"SelectManyBy{selectedFieldsString}"; break;
                case eStoredProcType.SelectAll: suffix = "SelectAll"; break;

                case eStoredProcType.Search: suffix = "Search"; break;

                case eStoredProcType.Update: suffix = "Update"; break;

                case eStoredProcType.DelSingle: suffix = "DeleteSingle"; break;
                case eStoredProcType.DelMany: suffix = "DeleteMany"; break;
                case eStoredProcType.DelAll: suffix = "DeleteAll"; break;

                default:
                    throw new Exception($"StoredProcType unknown: {procType}");
            }

            return $"[{table.Schema}].[{table.Name}_{suffix}]";
        }

        /// <summary>
        /// Converts a sql column name to a T-SQL variable name.
        /// Sample: Foo Bar -> @FooBar
        /// </summary>
        private static string ToTSQLVariableName(SqlColumn sqlColumn)
        {
            var buffer = Formatter.ToTitleCase(sqlColumn.Name, _UndesirableChars);
            return $"@{buffer}";
        }

        /// <summary>
        /// Generates a full qualified column name for use in a T-SQL script.
        /// </summary>
        private static string ToTSQLColumnName(SqlColumn sqlColumn, bool includeTableName = false)
        {
            if (includeTableName)
                return $"[{sqlColumn.Table.Name}].[{sqlColumn.Name}]";
            else
                return $"[{sqlColumn.Name}]";
        }

        /// <summary>
        /// Returns a default value in string format for any given SQL type.
        /// </summary>
        private static string SQLTypeDefaultValue(SqlColumn sqlColumn)
        {
            ArgumentNullException.ThrowIfNull(sqlColumn);

            string defaultValue = sqlColumn.SqlDataType switch
            {
                SqlDbType.BigInt => "42",
                SqlDbType.Binary => "0x0",
                SqlDbType.Bit => "1",
                SqlDbType.Char => "'a'",
                SqlDbType.DateTime => "'2013-01-25'",
                SqlDbType.Decimal => "3.14159",
                SqlDbType.Float => "3.14159",
                SqlDbType.Image => "0x0",
                SqlDbType.Int => "42",
                SqlDbType.Money => "0.99",
                SqlDbType.NChar => "'你'",
                SqlDbType.NText => "'你好'",
                SqlDbType.NVarChar => "'你好'",
                SqlDbType.Real => "3.14159",
                SqlDbType.UniqueIdentifier => "'DEADBEEF-DEAD-BEEF-DEAD-BEEFDEADBEEF'",
                SqlDbType.SmallDateTime => "'2013-01-25'",
                SqlDbType.SmallInt => "42",
                SqlDbType.SmallMoney => "0.99",
                SqlDbType.Text => "'Hello'",
                SqlDbType.Timestamp => "0x0",
                SqlDbType.TinyInt => "42",
                SqlDbType.VarBinary => "0x0",
                SqlDbType.VarChar => "'Hello'",
                SqlDbType.Variant => "NULL",
                SqlDbType.Xml => "'<xml></xml>'",
                SqlDbType.Udt => "NULL",
                SqlDbType.Structured => "NULL",
                SqlDbType.Date => "'2013-01-25'",
                SqlDbType.Time => "'23:36:00'",
                SqlDbType.DateTime2 => "'2013-01-25 23:36:00'",
                SqlDbType.DateTimeOffset => "'2013-01-25 23:36:00 +07:00'",

                _ => throw new ArgumentOutOfRangeException(nameof(sqlColumn.SqlDataType), $"Unknown SqlDbType: {sqlColumn.SqlDataType}")
            };

            return defaultValue;
        }

        /// <summary>
        /// Generates a parameter list for a stored procedure based on the columns in the table and the included fields option.
        /// 
        /// Example output:
        /// (
        ///    @CountryId INT OUTPUT,
        ///    @CountryName VARCHAR(255)
        /// )
        /// </summary>
        private string GenerateSqlStoredProcParameters(SqlTable sqlTable, eIncludedFields includedFields)
        {
            ArgumentNullException.ThrowIfNull(sqlTable);

            var sb = new StringBuilder();
            bool firstFlag = true;

            List<SqlColumn> cols = includedFields switch
            {
                eIncludedFields.All => sqlTable.ColumnList,
                eIncludedFields.PKOnly => sqlTable.PkList,
                eIncludedFields.NoIdentities => sqlTable.Columns.Values.Where(c => !c.IsIdentity).ToList(),
                eIncludedFields.None => new List<SqlColumn>(),

                _ => throw new Exception($"Unknown value for included fields: {includedFields}"),
            };

            sb.Append("(");

            foreach (var col in cols)
            {
                if (firstFlag)
                    firstFlag = false;
                else
                    sb.Append(",");

                sb.Append(Environment.NewLine);
                sb.Append(Formatter.AddTabs(1) + ToTSQLVariableName(col) + " " + ToTSQLType(col));

                if (col.IsIdentity)
                    sb.Append(" OUTPUT");
            }

            sb.Append(Environment.NewLine + ")");
            return sb.ToString();
        }

        private string GenerateStoredProcComments(SqlTable sqlTable, eStoredProcType procType, IEnumerable<string> selectedFields = null)
        {
            ArgumentNullException.ThrowIfNull(sqlTable);

            var sb = new StringBuilder();

            sb.AppendLine("/*");

            sb.AppendLine($"{Formatter.AddTabs(1)}Stored procedure for {sqlTable.FullName} object");
            sb.AppendLine();
            sb.Append($"{Formatter.AddTabs(1)}EXEC {GenerateSqlStoredProcName(sqlTable, procType, selectedFields)}");

            var firstFlag = true;

            foreach (var col in sqlTable.ColumnList)
            {
                if (firstFlag)
                    firstFlag = false;
                else
                    sb.Append(",");

                sb.Append($"{Environment.NewLine}{Formatter.AddTabs(1)}{ToTSQLVariableName(col)} = {SQLTypeDefaultValue(col)}");
            }

            sb.Append(Environment.NewLine);
            sb.Append("*/");

            return sb.ToString();
        }
    }
}

