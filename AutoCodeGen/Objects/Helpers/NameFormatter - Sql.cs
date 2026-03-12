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
using System.Data;
using System.Text.RegularExpressions;

namespace AutoCodeGen
{
    public static partial class NameFormatter
    {
        private static readonly string _SpNamePrefix = string.Empty;
        private static readonly string _SelectSingleByXSpSuffix = "SelectSingleBy{0}";
        private static readonly string _SelectManySpSuffix = "SelectMany";
        private static readonly string _SelectManyByXSpSuffix = "SelectManyBy{0}";
        private static readonly string _SelectAllSpSuffix = "SelectAll";
        private static readonly string _SearchPagedSpSuffix = "SearchAllPaged";
        private static readonly string _InsertSingleSpSuffix = "Insert";
        private static readonly string _UpdateSpSuffix = "Update";
        private static readonly string _UpdateInsertSpSuffix = "Set";
        private static readonly string _DelAllSpSuffix = "DeleteAll";
        private static readonly string _DelManySpSuffix = "DeleteMany";
        private static readonly string _DelSingleSpSuffix = "DeleteSingle";

        /// <summary>
        /// Returns the SQL column name formatted to be used as a T-SQL.
        /// 
        /// variable. Sample: Foo -> @Foo;
        /// </summary>
        public static string ToTSQLVariableName(SqlColumn sqlColumn)
        {
            return "@" + sqlColumn.Name;
        }

        /// <summary>
        /// Returns the SQL column name formatted for use in a T-SQL script.
        /// 
        /// Sample: Foo Bar -> [Foo Bar]
        /// </summary>
        public static string ToTSQLName(string input)
        {
            return $"[{input}]";
        }

        /// <summary>
        /// Returns a T-SQL representation of the SQL datatype.
        /// 
        /// Sample: VARCHAR(50)
        /// </summary>
        public static string ToTSQLType(SqlColumn sqlColumn)
        {
            SqlDbType sql_type = sqlColumn.SqlDataType;

            if (sql_type == SqlDbType.Binary || sql_type == SqlDbType.Char || sql_type == SqlDbType.NChar || sql_type == SqlDbType.NVarChar ||
                sql_type == SqlDbType.VarBinary || sql_type == SqlDbType.VarChar)
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
            else if (sql_type == SqlDbType.Decimal)
            {
                return $"{sqlColumn.SqlDataType.ToString().ToUpper()}({sqlColumn.Precision},{sqlColumn.Scale})";
            }
            else
            {
                return sqlColumn.SqlDataType.ToString().ToUpper();
            }
        }

        /// <summary>
        /// Returns the CSharp mapping of the SQL datatype.
        /// Maps actual datatypes, not datatype names.
        /// 
        /// sample: varchar(50) -> string
        /// </summary>
        public static string SQLTypeToCSharpType(SqlColumn sqlColumn)
        {
            if (sqlColumn.IsNullable)
            {
                switch (sqlColumn.SqlDataType)
                {
                    case SqlDbType.BigInt: return "long?";
                    case SqlDbType.Binary: return "byte[]";
                    case SqlDbType.Bit: return "bool?";
                    case SqlDbType.Char: return "string";
                    case SqlDbType.Date: return "DateTime?";
                    case SqlDbType.DateTime: return "DateTime?";
                    case SqlDbType.DateTime2: return "DateTime?";
                    case SqlDbType.DateTimeOffset: return "DateTime?";
                    case SqlDbType.Decimal: return "decimal?";
                    case SqlDbType.Float: return "double?";
                    case SqlDbType.Image: return "byte[]";
                    case SqlDbType.Int: return "int?";
                    case SqlDbType.Money: return "decimal?";
                    case SqlDbType.NChar: return "string";
                    case SqlDbType.NText: return "string";
                    case SqlDbType.NVarChar: return "string";
                    case SqlDbType.Real: return "float?";
                    case SqlDbType.SmallDateTime: return "DateTime?";
                    case SqlDbType.SmallInt: return "short?";
                    case SqlDbType.SmallMoney: return "float?";
                    case SqlDbType.Structured: return "// NO TYPE AVAILABLE FOR " + sqlColumn.SqlDataType.ToString();
                    case SqlDbType.Text: return "string";
                    case SqlDbType.Time: return "DateTime?";
                    case SqlDbType.Timestamp: return "string";
                    case SqlDbType.TinyInt: return "byte?";
                    case SqlDbType.Udt: return "byte[]";
                    case SqlDbType.UniqueIdentifier: return "Guid?";
                    case SqlDbType.VarBinary: return "byte[]";
                    case SqlDbType.VarChar: return "string";
                    case SqlDbType.Variant: return "byte[]";
                    case SqlDbType.Xml: return "string";

                    default:
                        return "// NO TYPE AVAILABLE FOR " + sqlColumn.SqlDataType.ToString();
                }
            }
            else
            {
                switch (sqlColumn.SqlDataType)
                {
                    case SqlDbType.BigInt: return "long";
                    case SqlDbType.Binary: return "byte[]";
                    case SqlDbType.Bit: return "bool";
                    case SqlDbType.Char: return "string";
                    case SqlDbType.Date: return "DateTime";
                    case SqlDbType.DateTime: return "DateTime";
                    case SqlDbType.DateTime2: return "DateTime";
                    case SqlDbType.DateTimeOffset: return "DateTime";
                    case SqlDbType.Decimal: return "decimal";
                    case SqlDbType.Float: return "double";
                    case SqlDbType.Image: return "byte[]";
                    case SqlDbType.Int: return "int";
                    case SqlDbType.Money: return "decimal";
                    case SqlDbType.NChar: return "string";
                    case SqlDbType.NText: return "string";
                    case SqlDbType.NVarChar: return "string";
                    case SqlDbType.Real: return "float";
                    case SqlDbType.SmallDateTime: return "DateTime";
                    case SqlDbType.SmallInt: return "short";
                    case SqlDbType.SmallMoney: return "float";
                    case SqlDbType.Structured: return "// NO TYPE AVAILABLE FOR " + sqlColumn.SqlDataType.ToString();
                    case SqlDbType.Text: return "string";
                    case SqlDbType.Time: return "DateTime";
                    case SqlDbType.Timestamp: return "string";
                    case SqlDbType.TinyInt: return "byte";
                    case SqlDbType.Udt: return "byte[]";
                    case SqlDbType.UniqueIdentifier: return "Guid";
                    case SqlDbType.VarBinary: return "byte[]";
                    case SqlDbType.VarChar: return "string";
                    case SqlDbType.Variant: return "byte[]";
                    case SqlDbType.Xml: return "string";

                    default:
                        return "// NO TYPE AVAILABLE FOR " + sqlColumn.SqlDataType.ToString();
                }
            }
        }

        /// <summary>
        /// Returns a string containing a column definition for a table creation script.
        /// 
        /// ex: [Id] [int] NULL
        /// </summary>
        public static string SQLTypeToColumnDefinition(SqlColumn sqlColumn)
        {
            string nullable;

            if (sqlColumn.IsNullable)
                nullable = "NULL";
            else
                nullable = "NOT NULL";

            return $"{ToTSQLName(sqlColumn.Name)} {ToTSQLType(sqlColumn)} {nullable}";
        }

        /// <summary>
        /// Returns a ASP.NET match of the SQL datatype
        /// </summary>
        public static string SQLToASPType(SqlColumn sqlColumn)
        {
            #region Sample
            //<UpdateParameters>
            //    <asp:Parameter Name="foo" Type="Boolean" />
            //    <asp:Parameter Name="foo" Type="Byte" />
            //    <asp:Parameter Name="foo" Type="Char" />
            //    <asp:Parameter Name="foo" Type="DateTime" />
            //    <asp:Parameter Name="foo" Type="DBNull" />
            //    <asp:Parameter Name="foo" Type="Decimal" />
            //    <asp:Parameter Name="foo" Type="Double" />
            //    <asp:Parameter Name="foo" Type="Empty" />
            //    <asp:Parameter Name="foo" Type="Int16" />
            //    <asp:Parameter Name="foo" Type="Int32" />
            //    <asp:Parameter Name="foo" Type="Int64" />
            //    <asp:Parameter Name="foo" Type="Object" />
            //    <asp:Parameter Name="foo" Type="SByte" />
            //    <asp:Parameter Name="foo" Type="Single" />
            //    <asp:Parameter Name="foo" Type="String" />
            //    <asp:Parameter Name="foo" Type="UInt16" /> 
            //    <asp:Parameter Name="foo" Type="UInt32" />
            //    <asp:Parameter Name="foo" Type="UInt64" />
            //</UpdateParameters>        
            #endregion

            switch (sqlColumn.SqlDataType)
            {
                case SqlDbType.BigInt: return "UInt64";
                case SqlDbType.Binary: return "Object";
                case SqlDbType.Bit: return "Boolean";
                case SqlDbType.Char: return "Char"; ;
                case SqlDbType.Date: return "DateTime";
                case SqlDbType.DateTime: return "DateTime";
                case SqlDbType.DateTime2: return "DateTime";
                case SqlDbType.DateTimeOffset: return "DateTime";
                case SqlDbType.Decimal: return "Decimal";
                case SqlDbType.Float: return "Decimal";
                case SqlDbType.Image: return "Object";
                case SqlDbType.Int: return "Int32";
                case SqlDbType.Money: return "Decimal"; ;
                case SqlDbType.NChar: return "string";
                case SqlDbType.NText: return "string";
                case SqlDbType.NVarChar: return "string";
                case SqlDbType.Real: return "Decimal";
                case SqlDbType.SmallDateTime: return "DateTime";
                case SqlDbType.SmallInt: return "Int16";
                case SqlDbType.SmallMoney: return "Decimal";
                case SqlDbType.Structured: return "Object";
                case SqlDbType.Text: return "string";
                case SqlDbType.Time: return "DateTime";
                case SqlDbType.Timestamp: return "string";
                case SqlDbType.TinyInt: return "Byte";
                case SqlDbType.Udt: return "Object";
                case SqlDbType.UniqueIdentifier: return "String";
                case SqlDbType.VarBinary: return "Object";
                case SqlDbType.VarChar: return "string";
                case SqlDbType.Variant: return "Object";
                case SqlDbType.Xml: return "string";

                default:
                    return "Int32";
            }
        }

        /// <summary>
        /// generates a legal SQL table name
        /// </summary>
        public static string FormatTableName(string input, string regex)
        {
            string output = input;

            if (string.IsNullOrWhiteSpace(regex))
            {
                output = Regex.Replace(output, regex, string.Empty, RegexOptions.IgnoreCase);
            }

            output = output.Replace(" ", "");
            output = output.Replace("-", "");
            output = output.Replace("_", "");

            return output;
        }
    }
}