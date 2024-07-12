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
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

using DAL.Standard.SqlMetadata;

namespace AutoCodeGenLibrary
{
    public static class NameFormatter
    {
        private static readonly string _SpNamePrefix = ConfigurationManager.AppSettings["SpNamePrefix"];
        private static readonly string _SelectSingleByXSpSuffix = ConfigurationManager.AppSettings["SelectSingleByXSpSuffix"];
        private static readonly string _SelectManySpSuffix = ConfigurationManager.AppSettings["SelectManySpSuffix"];
        private static readonly string _SelectManyByXSpSuffix = ConfigurationManager.AppSettings["SelectManyByXSpSuffix"];
        private static readonly string _SelectAllSpSuffix = ConfigurationManager.AppSettings["SelectAllSpSuffix"];
        private static readonly string _SearchPagedSpSuffix = ConfigurationManager.AppSettings["SearchPagedSpSuffix"];
        private static readonly string _InsertSingleSpSuffix = ConfigurationManager.AppSettings["InsertSingleSpSuffix"];
        private static readonly string _UpdateSpSuffix = ConfigurationManager.AppSettings["UpdateSpSuffix"];
        private static readonly string _UpdateInsertSpSuffix = ConfigurationManager.AppSettings["UpdateInsertSpSuffix"];
        private static readonly string _DelAllSpSuffix = ConfigurationManager.AppSettings["DelAllSpSuffix"];
        private static readonly string _DelManySpSuffix = ConfigurationManager.AppSettings["DelManySpSuffix"];
        private static readonly string _DelSingleSpSuffix = ConfigurationManager.AppSettings["DelSingleSpSuffix"];

        private static readonly string _CSharpClassPrefix = ConfigurationManager.AppSettings["CSharpClassPrefix"];
        private static readonly string _CSharpEnumPrefix = ConfigurationManager.AppSettings["CSharpEnumPrefix"];
        private static readonly string _CSharpInterfacePrefix = ConfigurationManager.AppSettings["CSharpInterfacePrefix"];

        private static readonly string[] _CSharpUndesireables = new string[] 
        { 
            "!", "$", "%", "^", "*", "(", ")", "-", "+", "\"", "=", "{", "}", "[",
            "]", ":", ";", "|", "'", "\\", "<", ">", ",", ".", "?", "/", " ", "~", "`" 
        };

        /// <summary>
        /// Returns the SQL column name formatted for UI eas of reading
        /// Sample: FooBar -> Foo Bar
        /// </summary>
        public static string ToFriendlyName(string input)
        {
            input = ToFriendlyCase(input);
            var cultureInfo = new CultureInfo("en-US");
            input = cultureInfo.TextInfo.ToTitleCase(input);

            return input;
        }

        /// <summary>
        /// Returns the SQL column name formatted for a C# class local member
        /// Sample: FooBar -> foo_bar
        /// </summary>
        public static string ToCSharpLocalVariable(string input)
        {
            return ToCamelCase(NormalizeForCSharp(input));
        }

        /// <summary>
        /// Returns the SQL column name formatted for a C# class private member
        /// Sample: Foo -> _Foo
        /// </summary>
        public static string ToCSharpPrivateVariable(string input)
        {
            return "_" + NormalizeForCSharp(input);
        }

        /// <summary>
        /// Returns the SQL column name formatted as a C# class name
        /// Sample: FooBar -> cFooBar
        /// </summary>
        public static string ToCSharpClassName(SqlTable input)
        {
            return _CSharpClassPrefix + NormalizeForCSharp(input.Name);
        }

        /// <summary>
        /// Returns the SQL column name formatted as a C# class name
        /// Sample: FooBar -> cFooBar
        /// </summary>
        public static string ToCSharpClassName(string input)
        {
            return _CSharpClassPrefix + NormalizeForCSharp(input);
        }

        /// <summary>
        /// Returns the SQL column name formatted as a C# interface name
        /// Sample: FooBar -> IFooBar
        /// </summary>
        public static string ToCSharpInterfaceName(string input)
        {
            return _CSharpInterfacePrefix + NormalizeForCSharp(input);
        }

        /// <summary>
        /// Returns the SQL column name formatted as a C# enum name
        /// Sample: FooBar -> eFooBar
        /// </summary>
        public static string ToCSharpEnumName(string input)
        {
            return _CSharpEnumPrefix + NormalizeForCSharp(input);
        }

        /// <summary>
        /// Returns the SQL column name formatted as a C# property name
        /// Sample: foo_bar -> FooBar
        /// </summary>
        public static string ToCSharpPropertyName(string input)
        {
            return NormalizeForCSharp(input);
        }

        /// <summary>
        /// Returns the SQL column name formatted as a C# property name
        /// Sample: foo_bar -> FooBar
        /// </summary>
        public static string ToCSharpPropertyName(SqlColumn input)
        {
            return NormalizeForCSharp(input.Name);
        }

        /// <summary>
        /// Returns the SQL column name formatted as a C# property name
        /// Includes a to string call if the property needs it
        /// Sample: foo_bar -> FooBar.ToString()
        /// </summary>
        public static string ToCSharpPropertyNameString(SqlColumn sqlColumn)
        {
            string output = NormalizeForCSharp(sqlColumn.Name);

            if (sqlColumn.BaseType == eSqlBaseType.String)
                return output;
            else
                return output + ".ToString()";
        }

        /// <summary>
        /// Generates a complete parameter string. 
        /// Sample: new SqlParameter() { ParameterName = "AccountId", SqlDbType = SqlDbType.Int, Value = obj.AccountId },
        /// </summary>
        public static string ToCSharpSQLParameterTypeString(SqlColumn sqlColumn, bool convertNullableFields)
        {
            string property_name = ToCSharpPropertyName(sqlColumn.Name);
            string column_value;

            // some columns might be nullable types
            if (sqlColumn.IsNullable && !convertNullableFields)
                column_value = $"(obj.{property_name} == null) ? (object)DBNull.Value : obj.{property_name}";
            else
                column_value = $"obj.{property_name}";

            // only set length on columns that can actually vary
            switch (sqlColumn.SqlDataType)
            {
                case SqlDbType.Text:
                case SqlDbType.NText:
                case SqlDbType.VarChar:
                case SqlDbType.NVarChar:
                case SqlDbType.VarBinary:
                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.Xml:
                    return $"new SqlParameter() {{ ParameterName = \"{ToTSQLVariableName(sqlColumn)}\", SqlDbType = {sqlColumn.SqlDataType.ToString()}, Value = {column_value}, Size = {sqlColumn.Length} }},";

                default:
                    return $"new SqlParameter() {{ ParameterName = \"{ToTSQLVariableName(sqlColumn)}\", SqlDbType = {sqlColumn.SqlDataType.ToString()}, Value = {column_value} }},";
            }
        }

        /// <summary>
        /// Returns a valid C# default value for the given SQL datatype.
        /// </summary>
        public static string GetCSharpDefaultValue(SqlColumn sqlColumn)
        {
            // do we have a non default value?
            if (sqlColumn.DefaultValue != string.Empty)
            {
                switch (sqlColumn.SqlDataType)
                {
                    case SqlDbType.BigInt: return sqlColumn.DefaultValue;
                    //case SqlDbType.Binary: return "null";
                    case SqlDbType.Bit:
                        if (sqlColumn.DefaultValue == "1")
                            return "true";
                        else
                            return "false";

                    case SqlDbType.Char: return "\"" + sqlColumn.DefaultValue + "\"";
                    case SqlDbType.Date: return (sqlColumn.DefaultValue.ToLower() == "getdate()") ? "DateTime.Now;" : $"DateTime.Parse(\"{sqlColumn.DefaultValue}\")";
                    case SqlDbType.DateTime: return (sqlColumn.DefaultValue.ToLower() == "getdate()") ? "DateTime.Now;" : $"DateTime.Parse(\"{sqlColumn.DefaultValue}\")";
                    case SqlDbType.DateTime2: return (sqlColumn.DefaultValue.ToLower() == "getdate()") ? "DateTime.Now;" : $"DateTime.Parse(\"{sqlColumn.DefaultValue}\")";
                    case SqlDbType.DateTimeOffset: return (sqlColumn.DefaultValue.ToLower() == "getdate()") ? "DateTime.Now;" : $"DateTime.Parse(\"{sqlColumn.DefaultValue}\")";
                    case SqlDbType.Decimal: return sqlColumn.DefaultValue;
                    case SqlDbType.Float: return sqlColumn.DefaultValue;
                    //case SqlDbType.Image:               return "null";
                    case SqlDbType.Int: return sqlColumn.DefaultValue;
                    case SqlDbType.Money: return sqlColumn.DefaultValue + "m";
                    case SqlDbType.NChar: return "\"" + sqlColumn.DefaultValue + "\"";
                    case SqlDbType.NText: return "\"" + sqlColumn.DefaultValue + "\"";
                    case SqlDbType.NVarChar: return "\"" + sqlColumn.DefaultValue + "\"";
                    case SqlDbType.Real: return sqlColumn.DefaultValue + "f";
                    case SqlDbType.SmallDateTime: return "\"" + DateTime.MinValue.ToString() + "\"";
                    case SqlDbType.SmallInt: return sqlColumn.DefaultValue;
                    case SqlDbType.SmallMoney: return sqlColumn.DefaultValue + "f";
                    //case SqlDbType.Structured:          return "null";         
                    case SqlDbType.Text: return "string.Empty";
                    case SqlDbType.Time: return "DateTime.Now";
                    //case SqlDbType.Timestamp:           return "string.Empty";
                    case SqlDbType.TinyInt: return sqlColumn.DefaultValue;
                    //case SqlDbType.Udt:                 return "null";
                    case SqlDbType.UniqueIdentifier: return "Guid.Empty";
                    //case SqlDbType.VarBinary:           return "null";
                    case SqlDbType.VarChar: return "\"" + sqlColumn.DefaultValue + "\"";
                    //case SqlDbType.Variant:             return "null";
                    case SqlDbType.Xml: return "\"" + sqlColumn.DefaultValue + "\"";

                    default:
                        return "// NO DEFAULT AVAILABLE FOR " + sqlColumn.SqlDataType.ToString();
                }

            }

            if (sqlColumn.IsNullable)
            {
                switch (sqlColumn.SqlDataType)
                {
                    case SqlDbType.Variant:
                        return "// NO DEFAULT AVAILABLE FOR " + sqlColumn.SqlDataType.ToString();

                    default:
                        return "null";
                }
            }
            else
            {
                switch (sqlColumn.SqlDataType)
                {
                    case SqlDbType.BigInt: return "0";
                    case SqlDbType.Binary: return "null";
                    case SqlDbType.Bit: return "false";
                    case SqlDbType.Char: return "string.Empty";
                    case SqlDbType.Date: return "DateTime.Now";
                    case SqlDbType.DateTime: return "DateTime.Now";
                    case SqlDbType.DateTime2: return "DateTime.Now";
                    case SqlDbType.DateTimeOffset: return "DateTime.Now";
                    case SqlDbType.Decimal: return "0.0m";
                    case SqlDbType.Float: return "0.0d";
                    case SqlDbType.Image: return "null";
                    case SqlDbType.Int: return "0";
                    case SqlDbType.Money: return "0.0m";
                    case SqlDbType.NChar: return "string.Empty";
                    case SqlDbType.NText: return "string.Empty";
                    case SqlDbType.NVarChar: return "string.Empty";
                    case SqlDbType.Real: return "0.0f";
                    case SqlDbType.SmallDateTime: return "DateTime.Now";
                    case SqlDbType.SmallInt: return "0";
                    case SqlDbType.SmallMoney: return "0.0f";
                    case SqlDbType.Structured: return "null";
                    case SqlDbType.Text: return "string.Empty";
                    case SqlDbType.Time: return "DateTime.Now";
                    case SqlDbType.Timestamp: return "string.Empty";
                    case SqlDbType.TinyInt: return "byte.MinValue";
                    case SqlDbType.Udt: return "null";
                    case SqlDbType.UniqueIdentifier: return "Guid.Empty";
                    case SqlDbType.VarBinary: return "null";
                    case SqlDbType.VarChar: return "string.Empty";
                    case SqlDbType.Variant: return "null";
                    case SqlDbType.Xml: return "string.Empty";

                    default:
                        return "// NO DEFAULT AVAILABLE FOR " + sqlColumn.SqlDataType.ToString();
                }
            }
        }

        /// <summary>
        /// Returns the proper cast to C# type for the given SQL datatype.
        /// Example: int -> Convert.ToInt32
        /// </summary>
        public static string GetCSharpCastString(SqlColumn sqlColumn)
        {
            //if (sql_column.IsNullable)
            //{
            //    // TODO
            //    return string.Empty;
            //}
            //else
            //{
            switch (sqlColumn.SqlDataType)
            {
                case SqlDbType.BigInt: return "Convert.ToInt64";
                //case SqlDbType.Binary: return Convert"null";
                case SqlDbType.Bit: return "Convert.ToBoolean";
                case SqlDbType.Char: return "Convert.ToChar";
                case SqlDbType.Date: return "Convert.ToDateTime";
                case SqlDbType.DateTime: return "Convert.ToDateTime";
                case SqlDbType.DateTime2: return "Convert.ToDateTime";
                case SqlDbType.DateTimeOffset: return "DateTime.Now";
                case SqlDbType.Decimal: return "Convert.ToDecimal";
                case SqlDbType.Float: return "Convert.ToDouble";
                //case SqlDbType.Image: return "null";
                case SqlDbType.Int: return "Convert.ToInt32";
                case SqlDbType.Money: return "Convert.ToDecimal";
                case SqlDbType.NChar: return "Convert.ToString";
                case SqlDbType.NText: return "Convert.ToString";
                case SqlDbType.NVarChar: return "Convert.ToString";
                case SqlDbType.Real: return "Convert.ToDouble";
                case SqlDbType.SmallDateTime: return "DateTime.Now";
                case SqlDbType.SmallInt: return "Convert.ToInt16";
                case SqlDbType.SmallMoney: return "Convert.ToDecimal";
                //case SqlDbType.Structured: return "null";         
                case SqlDbType.Text: return "Convert.ToString";
                case SqlDbType.Time: return "DateTime.Now";
                //case SqlDbType.Timestamp: return "string.Empty";
                case SqlDbType.TinyInt: return "byte.MinValue";
                //case SqlDbType.Udt: return "null";
                case SqlDbType.UniqueIdentifier: return "Convert.ToString";
                //case SqlDbType.VarBinary: return "null";
                case SqlDbType.VarChar: return "Convert.ToString";
                //case SqlDbType.Variant: return "null";
                case SqlDbType.Xml: return "Convert.ToString";

                default:
                    return "// NO CONVERSION AVAILABLE FOR " + sqlColumn.SqlDataType.ToString();
            }
            //}
        }

        /// <summary>
        /// Returns the minimum C# value for the given SQL datatype.
        /// </summary>
        public static string GetCSharpMinValue(SqlColumn sqlColumn)
        {
            switch (sqlColumn.SqlDataType)
            {
                case SqlDbType.BigInt: return "long.MinValue";
                case SqlDbType.Binary: return "null";
                case SqlDbType.Bit: return "false";
                case SqlDbType.Char: return "string.Empty";
                case SqlDbType.Date: return "DateTime.MinValue";
                case SqlDbType.DateTime: return "DateTime.MinValue";
                case SqlDbType.DateTime2: return "DateTime.MinValue";
                case SqlDbType.DateTimeOffset: return "DateTime.MinValue";
                case SqlDbType.Decimal: return "decimal.MinValue";
                case SqlDbType.Float: return "double.MinValue";
                case SqlDbType.Image: return "null";
                case SqlDbType.Int: return "int.MinValue";
                case SqlDbType.Money: return "decimal.MinValue";
                case SqlDbType.NChar: return "string.Empty";
                case SqlDbType.NText: return "string.Empty";
                case SqlDbType.NVarChar: return "string.Empty";
                case SqlDbType.Real: return "float.MinValue";
                case SqlDbType.SmallDateTime: return "DateTime.Now";
                case SqlDbType.SmallInt: return "int.MinValue";
                case SqlDbType.SmallMoney: return "float.MinValue";
                case SqlDbType.Structured: return "null";
                case SqlDbType.Text: return "string.Empty";
                case SqlDbType.Time: return "DateTime.MinValue";
                case SqlDbType.Timestamp: return "string.Empty";
                case SqlDbType.TinyInt: return "byte.MinValue";
                case SqlDbType.Udt: return "null";
                case SqlDbType.UniqueIdentifier: return "Guid.Empty";
                case SqlDbType.VarBinary: return "null";
                case SqlDbType.VarChar: return "string.Empty";
                case SqlDbType.Variant: return "null";
                case SqlDbType.Xml: return "string.Empty";

                default:
                    return "// NO MIN VALUE AVAILABLE FOR " + sqlColumn.SqlDataType.ToString();
            }
        }

        /// <summary>
        /// Returns the SQL column name formatted to be used as a T-SQL
        /// variable. Sample: Foo -> @Foo;
        /// </summary>
        public static string ToTSQLVariableName(SqlColumn sqlColumn)
        {
            return "@" + sqlColumn.Name;
        }

        /// <summary>
        /// Returns the SQL column name formatted for use in a T-SQL script.
        /// Sample: Foo Bar -> [Foo Bar]
        /// </summary>
        public static string ToTSQLName(string input)
        {
            return $"[{input}]";
        }

        /// <summary>
        /// Returns a T-SQL representation of the SQL datatype.
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
                return $"{sqlColumn.SqlDataType.ToString().ToUpper()}({sqlColumn.Precision.ToString()},{sqlColumn.Scale.ToString()})";
            }
            else
            {
                return sqlColumn.SqlDataType.ToString().ToUpper();
            }
        }

        /// <summary>
        /// Returns the CSharp mapping of the SQL datatype.
        /// Maps actual datatypes, not datatype names.
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
        /// Returns a string containing a column definition for a table creation script
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

        public static string NormalizeForCSharp(string input)
        {
            // SQL types allow for names that won't work for a C# name,
            // make sure that we have something that is legal.

            if (input != string.Empty)
            {
                // make a stab at replacing symbols that translate to words
                input = input.Replace("#", "Number");
                input = input.Replace("@", "At");
                input = input.Replace("&", "And");

                foreach (string forbiddenChar in _CSharpUndesireables)
                {
                    input = input.Replace(forbiddenChar, string.Empty);
                }

                // no leading numbers, foo!
                if (char.IsNumber(input[0]))
                {
                    input = "N" + input;
                }
            }

            return input;
        }

        /// <summary>
        /// Converts table column data to function argument string.
        /// Sample: string titles, string authors, int bookcount. 
        /// </summary>
        public static string GenerateCSharpFunctionArgs(SqlTable sqlTable, eIncludedFields includeTypes)
        {
            var sb = new StringBuilder();
            bool first_flag = true;

            foreach (SqlColumn sqlColumn in sqlTable.Columns.Values)
            {
                switch (includeTypes)
                {
                    case eIncludedFields.All:

                        if (first_flag)
                            first_flag = false;
                        else
                            sb.Append(", ");

                        sb.Append(SQLTypeToCSharpType(sqlColumn) + " " + ToCamelCase(sqlColumn.Name));
                        break;

                    case eIncludedFields.NoIdentities:

                        if (!sqlColumn.IsIdentity)
                        {
                            if (first_flag)
                                first_flag = false;
                            else
                                sb.Append(", ");

                            sb.Append(SQLTypeToCSharpType(sqlColumn) + " " + ToCamelCase(sqlColumn.Name));
                        }
                        break;

                    case eIncludedFields.PKOnly:

                        if (sqlColumn.IsPk)
                        {
                            if (first_flag)
                                first_flag = false;
                            else
                                sb.Append(", ");

                            sb.Append(SQLTypeToCSharpType(sqlColumn) + " " + ToCamelCase(sqlColumn.Name));
                        }
                        break;

                    default:
                        throw new Exception("eIncludedFields value " + includeTypes.ToString() + " is unrecognised..");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts table column data to function argument string.
        /// Does not incude function types.
        /// Sample: titles, authors, bookcount. 
        /// </summary>
        public static string GenerateCSharpFunctionList(SqlTable sqlTable, eIncludedFields includeTypes)
        {
            var sb = new StringBuilder();
            bool first_flag = true;

            foreach (SqlColumn sql_column in sqlTable.Columns.Values)
            {
                if (first_flag)
                    first_flag = false;
                else
                    sb.Append(", ");

                switch (includeTypes)
                {
                    case eIncludedFields.All:

                        sb.Append(ToCamelCase(sqlTable.Name));
                        break;

                    case eIncludedFields.NoIdentities:

                        if (!sql_column.IsIdentity)
                            sb.Append(ToCamelCase(sqlTable.Name));

                        break;

                    case eIncludedFields.PKOnly:

                        if (sql_column.IsPk)
                            sb.Append(ToCamelCase(sqlTable.Name));

                        break;

                    default:
                        throw new Exception("eIncludedFields value " + includeTypes.ToString() + " is unrecognised..");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Sample:
        /// [dbo].[Events_SelectSingle]
        /// </summary>
        public static string GenerateSqlStoredProcName(string tableName, eStoredProcType procType, IEnumerable<string> selectedFields)
        {
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentException("Cannot generate stored procedure name without a table name.");

            string suffix;
            string selectedFieldsString = (selectedFields == null) ? string.Empty : string.Join(string.Empty, selectedFields);

            switch (procType)
            {
                case eStoredProcType.SelectSingle: suffix = string.Format(_SelectSingleByXSpSuffix, selectedFieldsString); break;
                case eStoredProcType.SelectMany: suffix = _SelectManySpSuffix; break;
                case eStoredProcType.SelectManyByX: suffix = string.Format(_SelectManyByXSpSuffix, selectedFieldsString); break;
                case eStoredProcType.SelectAll: suffix = _SelectAllSpSuffix; break;
                case eStoredProcType.SearchPaged: suffix = _SearchPagedSpSuffix; break;
                case eStoredProcType.Insert: suffix = _InsertSingleSpSuffix; break;
                case eStoredProcType.Update: suffix = _UpdateSpSuffix; break;
                case eStoredProcType.UpdateInsert: suffix = _UpdateInsertSpSuffix; break;
                case eStoredProcType.DelSingle: suffix = _DelSingleSpSuffix; break;
                case eStoredProcType.DelMany: suffix = _DelManySpSuffix; break;
                case eStoredProcType.DelAll: suffix = _DelAllSpSuffix; break;

                default:
                    throw new Exception($"StoredProcType unknown: {procType}");
            }

            // tried title case here, gets a little odd.
            tableName = tableName.Replace(" ", "_");
            tableName = $"{_SpNamePrefix}{tableName}_{suffix}";
            tableName = tableName.Replace("__", "_");

            return tableName;
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

        /// <summary>
        /// Function to determine how many characters an input field should take in for a give data type.
        /// -1 returned if we cannot work it out.
        /// </summary>
        public static int ComputeStringLength(SqlColumn sqlColumn)
        {
            switch (sqlColumn.SqlDataType)
            {
                case SqlDbType.BigInt: return long.MaxValue.ToString().Length;
                case SqlDbType.Binary: return sqlColumn.Length;
                case SqlDbType.Bit: return 1;
                case SqlDbType.Char: return 1;
                //case SqlDbType.Date:		    	return "DateTime?";
                //case SqlDbType.DateTime:	    	return "DateTime?";
                //case SqlDbType.DateTime2:	    	return "DateTime?";
                //case SqlDbType.DateTimeOffset:  	return "DateTime?";
                case SqlDbType.Decimal: return decimal.MaxValue.ToString().Length;
                case SqlDbType.Float: return double.MaxValue.ToString().Length;
                //case SqlDbType.Image:               return "byte[]";
                case SqlDbType.Int: return int.MaxValue.ToString().Length;
                //case SqlDbType.Money:               return "decimal?";
                case SqlDbType.NChar: return sqlColumn.Length;
                //case SqlDbType.NText:               return "string";
                case SqlDbType.NVarChar: return sqlColumn.Length;
                //case SqlDbType.Real:                return "float?";
                //case SqlDbType.SmallDateTime:       return "DateTime?";
                case SqlDbType.SmallInt: return short.MaxValue.ToString().Length;
                //case SqlDbType.SmallMoney:          return "float?";          
                //case SqlDbType.Structured:          return "// NO TYPE AVAILABLE FOR " + sql_column.SqlDataType.ToString();         
                //case SqlDbType.Text:                return "string";
                //case SqlDbType.Time:                return "DateTime?";
                //case SqlDbType.Timestamp:           return "string";
                case SqlDbType.TinyInt: return byte.MaxValue.ToString().Length;
                //case SqlDbType.Udt:                 return "byte[]";
                //case SqlDbType.UniqueIdentifier:    return "Guid?";
                //case SqlDbType.VarBinary:           return "bool[]";
                case SqlDbType.VarChar: return sqlColumn.Length;
                //case SqlDbType.Variant:             return "byte[]"; 
                case SqlDbType.Xml: return sqlColumn.Length;

                default:
                    return -1;
            }
        }

        /// <summary>
        /// Converts a sql column name to a case that will work as a c# local variable.
        /// ex: FooBar -> foo_bar
        /// </summary>
        public static string ToSnakeCase(string input)
        {
            var sb = new StringBuilder();
            bool firstFlag = true;

            foreach (char c in input)
            {
                if (char.IsUpper(c) && !firstFlag)
                    sb.Append('_');

                sb.Append(c);

                if (firstFlag)
                    firstFlag = false;
            }

            return sb.ToString().ToLower();
        }

        /// <summary>
        /// Converts a sql column name to a case that will work as a c# local variable.
        /// ex: FooBar -> fooBar
        /// </summary>
        public static string ToCamelCase(string input)
        {
            var sb = new StringBuilder();
            bool firstFlag = true;
            bool nextCharUpper = false;

            foreach (char c in input)
            {
                if (firstFlag)
                {
                    firstFlag = false;
                    sb.Append(char.ToLower(c));
                }
                else
                {
                    if (c == ' ' || c == '_')
                    {
                        nextCharUpper = true;
                        continue;
                    }

                    if (nextCharUpper)
                    {
                        sb.Append(char.ToUpper(c));
                        nextCharUpper = false;
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts a sql column name to a readable case that will work in a comment.
        /// ex: FooBar -> Foo Bar
        /// </summary>
        public static string ToFriendlyCase(string input)
        {
            // FooBar -> Foo Bar

            var sb = new StringBuilder();
            bool first_flag = true;

            foreach (char c in input)
            {
                if (char.IsUpper(c) && !first_flag)
                    sb.Append(' ');

                sb.Append(c);

                if (first_flag)
                    first_flag = false;

            }

            return sb.ToString().ToLower();
        }
    }
}