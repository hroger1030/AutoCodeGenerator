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
using System.Configuration;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

using DAL.SqlMetadata;

namespace AutoCodeGenLibrary
{
    public static class NameFormatter
    {
        private static string s_CSharpClassPrefix = ConfigurationManager.AppSettings["CSharpClassPrefix"];
        private static string s_CSharpEnumPrefix = ConfigurationManager.AppSettings["CSharpEnumPrefix"];
        private static string s_CSharpInterfacePrefix = ConfigurationManager.AppSettings["CSharpInterfacePrefix"];

        private static string[] s_CSharpUndesireables = new string[] { "!", "$", "%", "^", "*", "(", ")", "-", "+", "\"", "=", "{", "}", "[", "]", ":", ";", "|", "'", "\\", "<", ">", ",", ".", "?", "/", " ", "~", "`" };

        /// <summary>
        /// Returns the SQL column name formatted for UI eas of reading
        /// Sample: FooBar -> Foo Bar
        /// </summary>
        public static string ToFriendlyName(string input)
        {
            input = ToFriendlyCase(input);

            System.Globalization.CultureInfo CI = new System.Globalization.CultureInfo("en-US");
            input = CI.TextInfo.ToTitleCase(input);

            return input;
        }

        /// <summary>
        /// Returns the SQL column name formatted for a C# class local member
        /// Sample: FooBar -> foo_bar
        /// </summary>
        public static string ToCSharpLocalVariable(string input)
        {
            return ToLocalCase(NormalizeForCSharp(input));
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
        public static string ToCSharpClassName(string input)
        {
            return s_CSharpClassPrefix + NormalizeForCSharp(input);
        }

        /// <summary>
        /// Returns the SQL column name formatted as a C# interface name
        /// Sample: FooBar -> IFooBar
        /// </summary>
        public static string ToCSharpInterfaceName(string input)
        {
            return s_CSharpInterfacePrefix + NormalizeForCSharp(input);
        }

        /// <summary>
        /// Returns the SQL column name formatted as a C# enum name
        /// Sample: FooBar -> eFooBar
        /// </summary>
        public static string ToCSharpEnumName(string input)
        {
            return s_CSharpEnumPrefix + NormalizeForCSharp(input);
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
        /// Includes a to string call if the property needs it
        /// Sample: foo_bar -> FooBar.ToString()
        /// </summary>
        public static string ToCSharpPropertyNameString(SqlColumn sql_column)
        {
            string output = NormalizeForCSharp(sql_column.Name);

            if (sql_column.BaseType == eSqlBaseType.String)
                return output;
            else
                return output + ".ToString()";
        }

        /// <summary>
        /// Generates a complete parameter string. 
        /// Sample: new SqlParameter() { ParameterName = "AccountId", SqlDbType = SqlDbType.Int, Value = obj.AccountId },
        /// </summary>
        public static string ToCSharpSQLParameterTypeString(SqlColumn sql_column, bool convertNullableFields)
        {
            string property_name = ToCSharpPropertyName(sql_column.Name);
            string column_value;

            // some columns might be nullable types
            if (sql_column.IsNullable && !convertNullableFields)
                column_value = $"(obj.{property_name} == null) ? (object)DBNull.Value : obj.{property_name}";
            else
                column_value = $"obj.{property_name};";
            
            // only set length on columns that can actually vary
            switch (sql_column.SqlDataType)
            {
                case SqlDbType.Text:
                case SqlDbType.NText:
                case SqlDbType.VarChar:
                case SqlDbType.NVarChar:
                case SqlDbType.VarBinary:
                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.Xml:
                    return $"new SqlParameter() {{ ParameterName = \"{ToTSQLVariableName(sql_column)}\", SqlDbType = {sql_column.SqlDataType.ToString()}, Value = {column_value}, Size = \"{sql_column.Length}\" }},";

                default:
                    return $"new SqlParameter() {{ ParameterName = \"{ToTSQLVariableName(sql_column)}\", SqlDbType = {sql_column.SqlDataType.ToString()}, Value = {column_value} }},";
            }
        }

        /// <summary>
        /// Returns a valid C# default value for the given SQL datatype.
        /// </summary>
        public static string GetCSharpDefaultValue(SqlColumn sql_column)
        {
            // do we have a non default value?
            if (sql_column.DefaultValue != string.Empty)
            {
                switch (sql_column.SqlDataType)
                {
                    case SqlDbType.BigInt: return sql_column.DefaultValue;
                    //case SqlDbType.Binary:		    	return "null";
                    case SqlDbType.Bit:
                        if (sql_column.DefaultValue == "1")
                            return "true";
                        else
                            return "false";

                    case SqlDbType.Char: return "\"" + sql_column.DefaultValue + "\"";
                    case SqlDbType.Date: return sql_column.DefaultValue.ToLower() == "getdate()" ? "DateTime.Now;" : "DateTime.Parse(\"" + sql_column.DefaultValue + "\")";
                    case SqlDbType.DateTime: return sql_column.DefaultValue.ToLower() == "getdate()" ? "DateTime.Now;" : "DateTime.Parse(\"" + sql_column.DefaultValue + "\")";
                    case SqlDbType.DateTime2: return sql_column.DefaultValue.ToLower() == "getdate()" ? "DateTime.Now;" : "DateTime.Parse(\"" + sql_column.DefaultValue + "\")";
                    case SqlDbType.DateTimeOffset: return sql_column.DefaultValue.ToLower() == "getdate()" ? "DateTime.Now;" : "DateTime.Parse(\"" + sql_column.DefaultValue + "\")";
                    case SqlDbType.Decimal: return sql_column.DefaultValue;
                    case SqlDbType.Float: return sql_column.DefaultValue;
                    //case SqlDbType.Image:               return "null";
                    case SqlDbType.Int: return sql_column.DefaultValue;
                    case SqlDbType.Money: return sql_column.DefaultValue + "m";
                    case SqlDbType.NChar: return "\"" + sql_column.DefaultValue + "\"";
                    case SqlDbType.NText: return "\"" + sql_column.DefaultValue + "\"";
                    case SqlDbType.NVarChar: return "\"" + sql_column.DefaultValue + "\"";
                    case SqlDbType.Real: return sql_column.DefaultValue + "f";
                    case SqlDbType.SmallDateTime: return "\"" + DateTime.MinValue.ToString() + "\"";
                    case SqlDbType.SmallInt: return sql_column.DefaultValue;
                    case SqlDbType.SmallMoney: return sql_column.DefaultValue + "f";
                    //case SqlDbType.Structured:          return "null";         
                    case SqlDbType.Text: return "string.Empty";
                    case SqlDbType.Time: return "DateTime.Now";
                    //case SqlDbType.Timestamp:           return "string.Empty";
                    case SqlDbType.TinyInt: return sql_column.DefaultValue;
                    //case SqlDbType.Udt:                 return "null";
                    case SqlDbType.UniqueIdentifier: return "Guid.Empty";
                    //case SqlDbType.VarBinary:           return "null";
                    case SqlDbType.VarChar: return "\"" + sql_column.DefaultValue + "\"";
                    //case SqlDbType.Variant:             return "null";
                    case SqlDbType.Xml: return "\"" + sql_column.DefaultValue + "\"";

                    default:
                        return "// NO DEFAULT AVAILABLE FOR " + sql_column.SqlDataType.ToString();
                }

            }

            if (sql_column.IsNullable)
            {
                switch (sql_column.SqlDataType)
                {
                    case SqlDbType.Variant:
                        return "// NO DEFAULT AVAILABLE FOR " + sql_column.SqlDataType.ToString();

                    default:
                        return "null";
                }
            }
            else
            {
                switch (sql_column.SqlDataType)
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
                        return "// NO DEFAULT AVAILABLE FOR " + sql_column.SqlDataType.ToString();
                }
            }
        }

        /// <summary>
        /// Returns the proper cast to C# type for the given SQL datatype.
        /// Example: int -> Convert.ToInt32
        /// </summary>
        public static string GetCSharpCastString(SqlColumn sql_column)
        {
            //if (sql_column.IsNullable)
            //{
            //    // TODO
            //    return string.Empty;
            //}
            //else
            //{
            switch (sql_column.SqlDataType)
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
                    return "// NO CONVERSION AVAILABLE FOR " + sql_column.SqlDataType.ToString();
            }
            //}
        }

        /// <summary>
        /// Returns the minimum C# value for the given SQL datatype.
        /// </summary>
        public static string GetCSharpMinValue(SqlColumn sql_column)
        {
            switch (sql_column.SqlDataType)
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
                    return "// NO MIN VALUE AVAILABLE FOR " + sql_column.SqlDataType.ToString();
            }
        }

        /// <summary>
        /// Returns the SQL column name formatted to be used as a T-SQL
        /// variable. Sample: Foo -> @Foo;
        /// </summary>
        public static string ToTSQLVariableName(SqlColumn sql_column)
        {
            return "@" + sql_column.Name;
        }

        /// <summary>
        /// Returns the SQL column name formatted for use in a T-SQL script.
        /// Sample: Foo Bar -> [Foo Bar]
        /// </summary>
        public static string ToTSQLName(string input)
        {
            return "[" + input + "]";
        }

        /// <summary>
        /// Returns a T-SQL representation of the SQL datatype.
        /// Sample: VARCHAR(50)
        /// </summary>
        public static string ToTSQLType(SqlColumn column)
        {
            SqlDbType sql_type = column.SqlDataType;

            if (sql_type == SqlDbType.Binary || sql_type == SqlDbType.Char || sql_type == SqlDbType.NChar || sql_type == SqlDbType.NVarChar ||
                sql_type == SqlDbType.VarBinary || sql_type == SqlDbType.VarChar)
            {
                if (column.Length == -1)
                {
                    return $"{column.SqlDataType.ToString().ToUpper()}(MAX)";
                }
                else
                {
                    return $"{column.SqlDataType.ToString().ToUpper()}({column.Length})";
                }
            }
            else if (sql_type == SqlDbType.Decimal)
            {
                return $"{column.SqlDataType.ToString().ToUpper()}({column.Precision.ToString()},{column.Scale.ToString()})";
            }
            else
            {
                return column.SqlDataType.ToString().ToUpper();
            }
        }

        /// <summary>
        /// Returns the CSharp mapping of the SQL datatype.
        /// Maps actual datatypes, not datatype names.
        /// sample: varchar(50) -> string
        /// </summary>
        public static string SQLTypeToCSharpType(SqlColumn sql_column)
        {
            if (sql_column.IsNullable)
            {
                switch (sql_column.SqlDataType)
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
                    case SqlDbType.Structured: return "// NO TYPE AVAILABLE FOR " + sql_column.SqlDataType.ToString();
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
                        return "// NO TYPE AVAILABLE FOR " + sql_column.SqlDataType.ToString();
                }
            }
            else
            {
                switch (sql_column.SqlDataType)
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
                    case SqlDbType.Structured: return "// NO TYPE AVAILABLE FOR " + sql_column.SqlDataType.ToString();
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
                        return "// NO TYPE AVAILABLE FOR " + sql_column.SqlDataType.ToString();
                }
            }
        }

        /// <summary>
        /// Returns a ASP.NET match of the SQL datatype
        /// </summary>
        public static string SQLToASPType(SqlColumn sql_column)
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

            switch (sql_column.SqlDataType)
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

                foreach (string character in s_CSharpUndesireables)
                {
                    input = input.Replace(character, string.Empty);
                }

                // no leading numbers, foo!
                if (Char.IsNumber(input[0]))
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
        public static string GenerateCSharpFunctionArgs(SqlTable sql_table, eIncludedFields include_types)
        {
            StringBuilder sb = new StringBuilder();
            bool first_flag = true;

            foreach (SqlColumn sql_column in sql_table.Columns.Values)
            {
                switch (include_types)
                {
                    case eIncludedFields.All:

                        if (first_flag)
                            first_flag = false;
                        else
                            sb.Append(", ");

                        sb.Append(NameFormatter.SQLTypeToCSharpType(sql_column) + " " + ToLocalCase(sql_column.Name));
                        break;

                    case eIncludedFields.NoIdentities:

                        if (!sql_column.IsIdentity)
                        {
                            if (first_flag)
                                first_flag = false;
                            else
                                sb.Append(", ");

                            sb.Append(NameFormatter.SQLTypeToCSharpType(sql_column) + " " + ToLocalCase(sql_column.Name));
                        }
                        break;

                    case eIncludedFields.PKOnly:

                        if (sql_column.IsPk)
                        {
                            if (first_flag)
                                first_flag = false;
                            else
                                sb.Append(", ");

                            sb.Append(NameFormatter.SQLTypeToCSharpType(sql_column) + " " + ToLocalCase(sql_column.Name));
                        }
                        break;

                    default:
                        throw new Exception("eIncludedFields value " + include_types.ToString() + " is unrecognised..");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts table column data to function argument string.
        /// Does not incude function types.
        /// Sample: titles, authors, bookcount. 
        /// </summary>
        public static string GenerateCSharpFunctionList(SqlTable sql_table, eIncludedFields include_types)
        {
            StringBuilder sb = new StringBuilder();
            bool first_flag = true;

            foreach (SqlColumn sql_column in sql_table.Columns.Values)
            {
                if (first_flag)
                    first_flag = false;
                else
                    sb.Append(", ");

                switch (include_types)
                {
                    case eIncludedFields.All:

                        sb.Append(ToLocalCase(sql_column.Name));
                        break;

                    case eIncludedFields.NoIdentities:

                        if (!sql_column.IsIdentity)
                            sb.Append(ToLocalCase(sql_column.Name));

                        break;

                    case eIncludedFields.PKOnly:

                        if (sql_column.IsPk)
                            sb.Append(ToLocalCase(sql_column.Name));

                        break;

                    default:
                        throw new Exception("eIncludedFields value " + include_types.ToString() + " is unrecognised..");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// generates a legal table name
        /// </summary>
        public static string FormatTableName(string input, string regex)
        {
            string output = input;

            if (string.IsNullOrWhiteSpace(regex))
            {
                input = Regex.Replace(input, regex, string.Empty, RegexOptions.IgnoreCase);
            }

            input = input.Replace(" ", "");
            input = input.Replace("-", "");
            input = input.Replace("_", "");

            return input;
        }

        /// <summary>
        /// Function to determine how many characters an input field should take in for a give data type.
        /// -1 returned if we cannot work it out.
        /// </summary>
        public static int ComputeStringLength(SqlColumn sql_column)
        {
            switch (sql_column.SqlDataType)
            {
                case SqlDbType.BigInt: return long.MaxValue.ToString().Length;
                case SqlDbType.Binary: return sql_column.Length;
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
                case SqlDbType.NChar: return sql_column.Length;
                //case SqlDbType.NText:               return "string";
                case SqlDbType.NVarChar: return sql_column.Length;
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
                case SqlDbType.VarChar: return sql_column.Length;
                //case SqlDbType.Variant:             return "byte[]"; 
                case SqlDbType.Xml: return sql_column.Length;

                default:
                    return -1;
            }
        }

        private static string ToTitleCase(string input)
        {
            // foo_bar -> FooBar
            // TODO: this function might need more testing..

            StringBuilder sb = new StringBuilder();
            bool first_flag = true;

            foreach (Char c in input)
            {
                if (first_flag)
                {
                    sb.Append(c);
                }
                else
                {
                    if (Char.IsUpper(c))
                    {
                        sb.Append("_");
                        sb.Append(c);
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
            }

            input = sb.ToString();

            input = input.Replace('_', ' ');

            //System.Globalization.CultureInfo CI = new System.Globalization.CultureInfo("en-US");
            //input = CI.TextInfo.ToTitleCase(input);

            input = input.Replace(" ", "");

            return input;
        }

        private static string ToLocalCase(string input)
        {
            // FooBar -> foo_bar

            StringBuilder sb = new StringBuilder();
            bool first_flag = true;

            foreach (Char c in input)
            {
                if (Char.IsUpper(c) && !first_flag)
                    sb.Append('_');

                sb.Append(c);

                if (first_flag)
                    first_flag = false;

            }

            return sb.ToString().ToLower();
        }

        private static string ToFriendlyCase(string input)
        {
            // FooBar -> Foo Bar

            StringBuilder sb = new StringBuilder();
            bool first_flag = true;

            foreach (Char c in input)
            {
                if (Char.IsUpper(c) && !first_flag)
                    sb.Append(' ');

                sb.Append(c);

                if (first_flag)
                    first_flag = false;

            }

            return sb.ToString().ToLower();
        }
    }
}