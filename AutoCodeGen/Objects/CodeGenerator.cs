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
using DAL;

namespace AutoCodeGenLibrary
{
    public partial class CodeGenerator
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(CodeGenerator));

        /// <summary>
        /// This object is a collection of characters that could create problems if they are used in c# variable names. 
        /// This object is a 'hit list' of characters to remove.
        /// </summary>
        private static string[] _CSharpUndesireables = new string[] { "!", "$", "%", "^", "*", "(", ")", "-", "+", "=", "{", "}", "[", "]", ":", ";", "|", "'", "<", ">", ",", ".", "?", "/", " ", "~", "`", "\"", "\\" };
        private static int[] _PrimeList = new int[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97, 101, 103, 107, 109, 113, 127, 131, 137, 139, 149, 151, 157, 163, 167, 173, 179, 181, 191, 193, 197, 199, 211, 223, 227, 229, 233, 239, 241, 251, 257, 263, 269, 271 };

        protected readonly static string SQL_PARAMETER_TEMPLATE = "parameters.Add(new SqlParameter() {{ ParameterName = \"{0}\", SqlDbType = SqlDbType.{1}, Size = {2}, Value = {3} }});";

        // figure out tab size
        internal static int _VisualStudioTabSize = Convert.ToInt32(ConfigurationManager.AppSettings["VisualStudioTabSize"]);

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
                //    var sb = new StringBuilder();
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
                sb.AppendLine(AddTabs(3) + "var sb = new StringBuilder();");
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
                            sb.AppendLine(AddTabs(4) + $"output ^= ({NameFormatter.ToCSharpPrivateVariable(sql_column.Name)} * {_PrimeList[prime]});");
                            break;

                        default:
                            sb.AppendLine(AddTabs(4) + $"output ^= ({NameFormatter.ToCSharpPrivateVariable(sql_column.Name)}.GetHashCode() * {_PrimeList[prime]});");
                            break;
                    }

                    prime++;

                    // my what a wide table you have...
                    if (prime > _PrimeList.Length)
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

            var sb = new StringBuilder();

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

            var sb = new StringBuilder();

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

            var sb = new StringBuilder();

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

            var sb = new StringBuilder();

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

            var sb = new StringBuilder();

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
                foreach (string character in _CSharpUndesireables)
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

            var sb = new StringBuilder();

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
            sb.AppendLine(AddTabs(4) + "var sb = new StringBuilder();");
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

        // Webservice Generation
        public static OutputObject GenerateWebServiceCodeInfrontClass(SqlTable sqlTable)
        {
            if (sqlTable == null)
                return null;

            string class_name = NameFormatter.ToCSharpClassName(sqlTable.Name);

            OutputObject output = new OutputObject();
            output.Name = class_name + ".asmx";
            output.Type = OutputObject.eObjectType.WebService;

            var sb = new StringBuilder();

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

            var sb = new StringBuilder();

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

            var sb = new StringBuilder();

            sb.AppendLine(GenerateDescriptionHeader(database_name));
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

            var sb = new StringBuilder();

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

            var sb = new StringBuilder();

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

            var sb = new StringBuilder();

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

            var sb = new StringBuilder();

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

            var sb = new StringBuilder();

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

            var sb = new StringBuilder();

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

            var sb = new StringBuilder();

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

            var sb = new StringBuilder();

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

            string query = "SELECT * FROM [" + database_name + "].[dbo].[" + table_name + "]";
            var db = new Database(connection_string);

            DataTable data_table = db.ExecuteQuery(query, null);
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

            var sb = new StringBuilder();

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

        // formatting methods
        private static string PadCSharpVariableName(string input, int longest_string_length)
        {
            return PadVariableName(input, longest_string_length, 0, _VisualStudioTabSize);
        }
        private static string PadCSharpVariableName(string input, int longest_string_length, int padding)
        {
            return PadVariableName(input, longest_string_length, padding, _VisualStudioTabSize);
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