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
using System.Text;

namespace AutoCodeGen
{
    public static partial class NameFormatter
    {
        private static HashSet<char> _CSharpUndesirables = new()
        {
            '!', '$', '%', '^', '*', '(', ')', '-', '+', '\\', '=',
            '{', '}', '[', ']', ':', ';', '|', '\'', '<', '>', ',',
            '.', '?', '/', '~', '`', '@', '#', '"', ' ', '\t', '&'
        };

        /// <summary>
        /// Returns the SQL column name formatted for a C# class local member.
        /// 
        /// Sample: FooBar -> foo_bar
        /// </summary>
        public static string ToCSharpLocalVariable(string input)
        {
            return ToCamelCase(input, _CSharpUndesirables);
        }

        /// <summary>
        /// Returns the SQL column name formatted for a C# class private member.
        /// 
        /// Sample: Foo -> _Foo
        /// </summary>
        public static string ToCSharpPrivateVariable(string input)
        {
            var buffer = RemoveInvalidCharacters(input, _CSharpUndesirables);
            return $"_{buffer}";
        }

        /// <summary>
        /// Returns the SQL column name formatted as a C# class name.
        /// 
        /// Sample: FooBar -> cFooBar
        /// </summary>
        public static string ToCSharpClassName(string input)
        {
            return ToTitleCase(input, _CSharpUndesirables);
        }

        /// <summary>
        /// Returns the SQL column name formatted as a C# interface name.
        /// 
        /// Sample: FooBar -> IFooBar
        /// </summary>
        public static string ToCSharpInterfaceName(string input)
        {
            var buffer = ToTitleCase(input, _CSharpUndesirables);
            return $"I{buffer}";
        }

        /// <summary>
        /// Returns the SQL column name formatted as a C# enum name.
        /// 
        /// Sample: FooBar -> eFooBar
        /// </summary>
        public static string ToCSharpEnumName(string input)
        {
            var buffer = ToTitleCase(input, _CSharpUndesirables);
            return $"e{buffer}";
        }
       
        /// <summary>
        /// Returns the SQL column name formatted as a C# property name.
        /// 
        /// Sample: foo_bar -> FooBar
        /// </summary>
        public static string ToCSharpPropertyName(string input)
        {
            return ToTitleCase(input, _CSharpUndesirables);
        }

        /// <summary>
        /// Returns the SQL column name formatted as a C# property name.
        /// Includes a to string call if the property needs it.
        /// 
        /// Sample: foo_bar -> FooBar.ToString()
        /// </summary>
        public static string ToCSharpPropertyNameAsString(SqlColumn sqlColumn)
        {
            var buffer = ToTitleCase(sqlColumn.Name, _CSharpUndesirables);

            if (sqlColumn.BaseType == eSqlBaseType.String)
                return buffer;
            else
                return $"{buffer}.ToString()";
        }

        /// <summary>
        /// Generates a complete parameter string. 
        /// 
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
                    return $"new SqlParameter() {{ ParameterName = \"{ToTSQLVariableName(sqlColumn)}\", SqlDbType = {sqlColumn.SqlDataType}, Value = {column_value}, Size = {sqlColumn.Length} }},";

                default:
                    return $"new SqlParameter() {{ ParameterName = \"{ToTSQLVariableName(sqlColumn)}\", SqlDbType = {sqlColumn.SqlDataType}, Value = {column_value} }},";
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
                    case SqlDbType.Date: return sqlColumn.DefaultValue.ToLower() == "getdate()" ? "DateTime.Now;" : $"DateTime.Parse(\"{sqlColumn.DefaultValue}\")";
                    case SqlDbType.DateTime: return sqlColumn.DefaultValue.ToLower() == "getdate()" ? "DateTime.Now;" : $"DateTime.Parse(\"{sqlColumn.DefaultValue}\")";
                    case SqlDbType.DateTime2: return sqlColumn.DefaultValue.ToLower() == "getdate()" ? "DateTime.Now;" : $"DateTime.Parse(\"{sqlColumn.DefaultValue}\")";
                    case SqlDbType.DateTimeOffset: return sqlColumn.DefaultValue.ToLower() == "getdate()" ? "DateTime.Now;" : $"DateTime.Parse(\"{sqlColumn.DefaultValue}\")";
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
        /// 
        /// Example: int -> Convert.ToInt32
        /// </summary>
        public static string GetCSharpCastString(SqlColumn sqlColumn)
        {
            // todo: do we need to deal with a nullable type here?
            // need to check use of Binary, VarBinary, Image to make sure cast is correct

            switch (sqlColumn.SqlDataType)
            {
                case SqlDbType.BigInt: return "Convert.ToInt64";
                case SqlDbType.Binary: return "(byte[])";
                case SqlDbType.Bit: return "Convert.ToBoolean";
                case SqlDbType.Char: return "Convert.ToChar";
                case SqlDbType.Date: return "Convert.ToDateTime";
                case SqlDbType.DateTime: return "Convert.ToDateTime";
                case SqlDbType.DateTime2: return "Convert.ToDateTime";
                case SqlDbType.DateTimeOffset: return "DateTimeOffset.Parse";
                case SqlDbType.Decimal: return "Convert.ToDecimal";
                case SqlDbType.Float: return "Convert.ToDouble";
                case SqlDbType.Image: return "(byte[])";
                case SqlDbType.Int: return "Convert.ToInt32";
                case SqlDbType.Money: return "Convert.ToDecimal";
                case SqlDbType.NChar: return "Convert.ToString";
                case SqlDbType.NText: return "Convert.ToString";
                case SqlDbType.NVarChar: return "Convert.ToString";
                case SqlDbType.Real: return "Convert.ToSingle";
                case SqlDbType.SmallDateTime: return "Convert.ToDateTime";
                case SqlDbType.SmallInt: return "Convert.ToInt16";
                case SqlDbType.SmallMoney: return "Convert.ToDecimal";
                //case SqlDbType.Structured: return "null";         
                case SqlDbType.Text: return "Convert.ToString";
                case SqlDbType.Time: return "TimeSpan.Parse";
                //case SqlDbType.Timestamp: return "string.Empty";
                case SqlDbType.TinyInt: return "Convert.ToByte";
                //case SqlDbType.Udt: return "null";
                case SqlDbType.UniqueIdentifier: return "Convert.ToString";
                case SqlDbType.VarBinary: return "(byte[])";
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
                    return $"// NO MIN VALUE AVAILABLE FOR {sqlColumn.SqlDataType}";
            }
        }

        /// <summary>
        /// Converts table column data to function argument string.
        /// Sample: string titles, string authors, int bookCount. 
        /// </summary>
        public static string GenerateCSharpFunctionArgs(SqlTable sqlTable, eIncludedFields includeTypes, HashSet<char> undesirables)
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

                        sb.Append(SQLTypeToCSharpType(sqlColumn) + " " + ToCamelCase(sqlColumn.Name, undesirables));
                        break;

                    case eIncludedFields.NoIdentities:

                        if (!sqlColumn.IsIdentity)
                        {
                            if (first_flag)
                                first_flag = false;
                            else
                                sb.Append(", ");

                            sb.Append(SQLTypeToCSharpType(sqlColumn) + " " + ToCamelCase(sqlColumn.Name, undesirables));
                        }
                        break;

                    case eIncludedFields.PKOnly:

                        if (sqlColumn.IsPk)
                        {
                            if (first_flag)
                                first_flag = false;
                            else
                                sb.Append(", ");

                            sb.Append(SQLTypeToCSharpType(sqlColumn) + " " + ToCamelCase(sqlColumn.Name, undesirables));
                        }
                        break;

                    default:
                        throw new Exception("eIncludedFields value " + includeTypes.ToString() + " is unrecognized.");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts table column data to function argument string.
        /// Does not include function types.
        /// Sample: titles, authors, bookCount. 
        /// </summary>
        public static string GenerateCSharpFunctionList(SqlTable sqlTable, eIncludedFields includeTypes, HashSet<char> undesirables)
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

                        sb.Append(ToCamelCase(sqlTable.Name, undesirables));
                        break;

                    case eIncludedFields.NoIdentities:

                        if (!sql_column.IsIdentity)
                            sb.Append(ToCamelCase(sqlTable.Name, undesirables));

                        break;

                    case eIncludedFields.PKOnly:

                        if (sql_column.IsPk)
                            sb.Append(ToCamelCase(sqlTable.Name, undesirables));

                        break;

                    default:
                        throw new Exception("eIncludedFields value " + includeTypes.ToString() + " is unrecognized.");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Sample: [dbo].[Events_SelectSingle]
        /// </summary>
        public static string GenerateSqlStoredProcName(string tableName, eStoredProcType procType, IEnumerable<string> selectedFields)
        {
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentException("Cannot generate stored procedure name without a table name.");

            string suffix;
            string selectedFieldsString = selectedFields == null ? string.Empty : string.Join(string.Empty, selectedFields);

            switch (procType)
            {
                case eStoredProcType.SelectSingle: suffix = $"{_SelectSingleByXSpSuffix}{selectedFieldsString}"; break;
                case eStoredProcType.SelectMany: suffix = _SelectManySpSuffix; break;
                case eStoredProcType.SelectManyByX: suffix = $"{_SelectManyByXSpSuffix}{selectedFieldsString}"; break;
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
            tableName = $"{_SpNamePrefix}{tableName}_{suffix}";
            tableName = tableName.Replace(" ", "_");
            tableName = tableName.Replace("__", "_");

            return tableName;
        }
    }
}