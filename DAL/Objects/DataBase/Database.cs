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
using System.Data.SqlClient;
using System.Reflection;
using System.Text;

namespace DAL
{
    public class Database : IDatabase
    {
        #region Sample Parameter Useage

            // List<SqlParameter> parameters = new List<SqlParameter>();
            // parameters.Add(new SqlParameter {ParameterName = "@OrderNumber", SqlDbType = SqlDbType.VarChar, Size = 50, Value = txtOrderNumber.Text };);

        #endregion

        #region Constants

            /// <summary>
            /// This is the name of the sql parameter that is added to each parameter list
            /// automatically to recieve return values.
            /// </summary>
            private static readonly string RETURN_VALUE_NAME = "@ReturnValue";

        #endregion

        #region Fields

            protected string _ConnectionString;

        #endregion

        #region Functions

            public Database(string connection_string)
            {
                _ConnectionString = connection_string;
            }

            public DataTable ExecuteQuery(string sql_query, List<SqlParameter> parameters)
            {
                return Database.ExecuteQuery(sql_query, parameters, _ConnectionString, false);
            }

            public DataTable ExecuteQuerySp(string sql_query, List<SqlParameter> parameters)
            {
                return Database.ExecuteQuery(sql_query, parameters, _ConnectionString, true);
            }

            public List<T> ExecuteQuery<T>(string sql_query, List<SqlParameter> parameters) where T : class, new()
            {
                return Database.ExecuteQuery<T>(sql_query, parameters, _ConnectionString, false);
            }

            public List<T> ExecuteQuerySp<T>(string sql_query, List<SqlParameter> parameters) where T : class, new()
            {
                return Database.ExecuteQuery<T>(sql_query, parameters, _ConnectionString, true);
            }

            public int ExecuteNonQuery(string sql_query, List<SqlParameter> parameters)
            {
                return Database.ExecuteNonQuery(sql_query, parameters, _ConnectionString, false);
            }

            public int ExecuteNonQuerySp(string sql_query, List<SqlParameter> parameters)
            {
                return Database.ExecuteNonQuery(sql_query, parameters, _ConnectionString, true);
            }

            public T ExecuteScalar<T>(string sql_query, List<SqlParameter> parameters)
            {
                return Database.ExecuteScalar<T>(sql_query, parameters, _ConnectionString, false);
            }

            public T ExecuteScalarSp<T>(string sql_query, List<SqlParameter> parameters)
            {
                return Database.ExecuteScalar<T>(sql_query, parameters, _ConnectionString, true);
            }        


            public static DataTable ExecuteQuery(string sql_query, List<SqlParameter> parameters, string connection)
            {
                return ExecuteQuery(sql_query, parameters, connection, false);
            }

            public static DataTable ExecuteQuerySp(string sql_query, List<SqlParameter> parameters, string connection)
            {
                return ExecuteQuery(sql_query, parameters, connection, true);
            }

            public static List<T> ExecuteQuery<T>(string sql_query, List<SqlParameter> parameters, string connection) where T : class, new()
            {
                return ExecuteQuery<T>(sql_query, parameters, connection, false);
            }

            public static List<T> ExecuteQuerySp<T>(string sql_query, List<SqlParameter> parameters, string connection) where T : class, new()
            {
                return ExecuteQuery<T>(sql_query, parameters, connection, true);
            }

            public static int ExecuteNonQuery(string sql_query, List<SqlParameter> parameters, string connection)
            {
                return ExecuteNonQuery(sql_query, parameters, connection, false);
            }

            public static int ExecuteNonQuerySp(string sql_query, List<SqlParameter> parameters, string connection)
            {
                return ExecuteNonQuery(sql_query, parameters, connection, true);
            }

            public static T ExecuteScalar<T>(string sql_query, List<SqlParameter> parameters, string connection)
            {
                return ExecuteScalar<T>(sql_query, parameters, connection, false);
            }

            public static T ExecuteScalarSp<T>(string sql_query, List<SqlParameter> parameters, string connection)
            {
                return ExecuteScalar<T>(sql_query, parameters, connection, true);
            }


            private static DataTable ExecuteQuery(string sql_query, List<SqlParameter> parameters, string connection, bool stored_procedure)
            {
                if (string.IsNullOrWhiteSpace(sql_query))
                    throw new ArgumentException("Query string is null or empty.");

                if (string.IsNullOrWhiteSpace(connection))
                    throw new ArgumentException("Connection string is null or empty.");

                parameters = AddReturnValueParameter(parameters);

                using (SqlConnection conn = new SqlConnection(connection))
                {
                    using (SqlCommand cmd = new SqlCommand(sql_query, conn))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter())
                        {
                            cmd.CommandType = (stored_procedure) ? CommandType.StoredProcedure : CommandType.Text;

                            if (parameters != null)
                            {
                                foreach (SqlParameter parameter in parameters)
                                    cmd.Parameters.Add(parameter);
                            }

                            #if (DEBUG)
                            string SQL_debug_string = GenerateSqlDebugString(sql_query, parameters);
                            #endif

                            DataTable dt = new DataTable();

                            conn.Open();
                            adapter.SelectCommand = cmd;
                            adapter.Fill(dt);
                            conn.Close();

                            if (parameters != null)
                            {
                                for (int i = 0; i < cmd.Parameters.Count; i++)
                                    parameters[i].Value = cmd.Parameters[i].Value;
                            }

                            return dt;
                        }
                    }
                }
            }

            private static List<T> ExecuteQuery<T>(string sql_query, List<SqlParameter> parameters, string connection, bool stored_procedure) where T : class, new()
            {
                if (string.IsNullOrWhiteSpace(sql_query))
                    throw new ArgumentException("Query string is null or empty.");

                if (string.IsNullOrWhiteSpace(connection))
                    throw new ArgumentException("Connection string is null or empty.");

                parameters = AddReturnValueParameter(parameters);

                Type object_type = typeof(T);
                List<T> output = new List<T>();

                using (SqlConnection conn = new SqlConnection(connection))
                {
                    using (SqlCommand cmd = new SqlCommand(sql_query, conn))
                    {
                        cmd.CommandType = (stored_procedure) ? CommandType.StoredProcedure : CommandType.Text;

                        if (parameters != null)
                        {
                            foreach (SqlParameter parameter in parameters)
                                cmd.Parameters.Add(parameter);
                        }

                        #if (DEBUG)
                        string SQL_debug_string = GenerateSqlDebugString(sql_query, parameters);
                        #endif

                        conn.Open();

                        SqlDataReader data_reader = cmd.ExecuteReader();
                        output = ParseDatareaderResult<T>(data_reader, true);

                        data_reader.Close();
                        conn.Close();

                        if (parameters != null)
                        {
                            for (int i = 0; i < cmd.Parameters.Count; i++)
                                parameters[i].Value = cmd.Parameters[i].Value;
                        }

                        return output;
                    }
                }
            }

            private static int ExecuteNonQuery(string sql_query, List<SqlParameter> parameters, string connection, bool stored_procedure)
            {
                if (string.IsNullOrWhiteSpace(sql_query))
                    throw new ArgumentException("Query string is null or empty.");

                if (string.IsNullOrWhiteSpace(connection))
                    throw new ArgumentException("Connection string is null or empty.");

                using (SqlConnection conn = new SqlConnection(connection))
                {
                    using (SqlCommand cmd = new SqlCommand(sql_query, conn))
                    {
                        cmd.CommandType = (stored_procedure) ? CommandType.StoredProcedure : CommandType.Text;

                        if (parameters != null)
                        {
                            foreach (SqlParameter parameter in parameters)
                                cmd.Parameters.Add(parameter);
                        }

                        #if (DEBUG)
                        string SQL_debug_string = GenerateSqlDebugString(sql_query, parameters);
                        #endif

                        conn.Open();
                        int return_value = cmd.ExecuteNonQuery();
                        conn.Close();

                        if (parameters != null)
                        {
                            for (int i = 0; i < cmd.Parameters.Count; i++)
                                parameters[i].Value = cmd.Parameters[i].Value;
                        }

 			            return return_value;
                    }
                }
            }

            private static T ExecuteScalar<T>(string sql_query, List<SqlParameter> parameters, string connection, bool stored_procedure)
            {
                if (string.IsNullOrWhiteSpace(sql_query))
                    throw new ArgumentException("Query string is null or empty.");

                if (string.IsNullOrWhiteSpace(connection))
                    throw new ArgumentException("Connection string is null or empty.");

                parameters = AddReturnValueParameter(parameters);

                using (SqlConnection conn = new SqlConnection(connection))
                {
                    using (SqlCommand cmd = new SqlCommand(sql_query, conn))
                    {
                        T results = default(T);

                        cmd.CommandType = (stored_procedure) ? CommandType.StoredProcedure : CommandType.Text;

                        if (parameters != null)
                        {
                            foreach (SqlParameter parameter in parameters)
                                cmd.Parameters.Add(parameter);
                        }

                        #if (DEBUG)
                        string SQL_debug_string = GenerateSqlDebugString(sql_query, parameters);
                        #endif

                        conn.Open();

                        object buffer = cmd.ExecuteScalar();

                        if (buffer != null) 
                        {
                            Type type = buffer.GetType();

                            if (type != typeof(DBNull) || (type == typeof(DBNull) && results is Nullable))
                                results = (T)buffer;
                        }

                        conn.Close();

                        if (parameters != null)
                        {
                            for (int i = 0; i < cmd.Parameters.Count; i++)
                                parameters[i].Value = cmd.Parameters[i].Value;
                        }

                        return results;
                    }
                }

            } 

            public static DataTable GetSchema(string connection_string)
            {
                if (string.IsNullOrWhiteSpace(connection_string))
                    throw new ArgumentException("Connection string is null or empty.");

                using (SqlConnection conn = new SqlConnection(connection_string))
                {
                    DataTable dt = null;

                    conn.Open();
                    dt = conn.GetSchema("Databases");
                    conn.Close();

                    return dt;
                }
            }

            /// <summary>
            /// Converts a list of IEnumerable objects to a string of comma delimited items.
            /// </summary>
            public static string GenericListToStringList<T>(IEnumerable<T> list)
            {
                return GenericListToStringList<T>(list, null, null);
            }

            /// <summary>
            /// Converts a list of IEnumerable objects to a string of comma delimited items. If a quote_character
            /// is defined, this will wrap each item with the character(s) passed.
            /// </summary>
            public static string GenericListToStringList<T>(IEnumerable<T> list, string quote_character, string quote_escape_character)
            {
                if (list == null)
                    throw new ArgumentException("Cannot convert a null IEnumerable object");

                StringBuilder sb = new StringBuilder();
                bool first_flag = true;

                foreach (T item in list)
                {
                    if (first_flag)
                        first_flag = false;
                    else
                        sb.Append(",");

                    if (item == null)
                    {
                        sb.Append("null");
                    }
                    else
                    {
                        string buffer = item.ToString();

                        if (!string.IsNullOrWhiteSpace(quote_escape_character))
                            buffer = buffer.Replace(quote_character, quote_escape_character);

                        if (!string.IsNullOrWhiteSpace(quote_character))
                            sb.Append(quote_character + buffer + quote_character);
                        else
                            sb.Append(buffer);
                    }
                }

                return sb.ToString();
            }

            /// <summary>
            /// Method creates sql debugging strings with parameterized argument lists
            /// </summary>
            private static string GenerateSqlDebugString(string query, List<SqlParameter> parameter_list)
            {
                if (parameter_list == null)
                    return string.Empty;

                List<string> value_list = new List<string>();

                foreach (var item in parameter_list)
                {
                    if (item.Direction == ParameterDirection.Input || item.Direction == ParameterDirection.InputOutput)
                    {
                        if (item.IsNullable)
                        {
                            value_list.Add(string.Format("{0} = null", item.ParameterName));
                        }
                        else
                        {
                            switch (item.SqlDbType)
                            {
                                case SqlDbType.Char:
                                case SqlDbType.NChar:
                                case SqlDbType.Text:
                                case SqlDbType.NText:
                                case SqlDbType.NVarChar:
                                case SqlDbType.VarChar:
                                case SqlDbType.UniqueIdentifier:
                                case SqlDbType.DateTime:
                                case SqlDbType.Date:
                                case SqlDbType.Time:
                                case SqlDbType.DateTime2:
                                    value_list.Add(string.Format("{0} = '{1}'", item.ParameterName, item.Value));
                                    break;

                                default:
                                    value_list.Add(string.Format("{0} = {1}", item.ParameterName, item.Value)); 
                                    break;
                            }
                        }
                    }
                }

                return query + " " + GenericListToStringList(value_list,null,null);
            }

            private static List<SqlParameter> AddReturnValueParameter(List<SqlParameter> parameters)
            {
                if (parameters == null)
                    parameters = new List<SqlParameter>();

                var result = parameters.Find(a => a.ParameterName == RETURN_VALUE_NAME);

                if (result == null)
                    parameters.Add(new SqlParameter {ParameterName = RETURN_VALUE_NAME, SqlDbType = SqlDbType.Int, Size = -1, Value = -1, Direction = ParameterDirection.ReturnValue });

                return parameters;
            }

            private static List<T> ParseDatareaderResult<T>(SqlDataReader data_reader, bool throw_unmapped_fields_error) where T : class, new()
            {
                Type output_type = typeof(T);
                List<T> results = new List<T>();
                Dictionary<string, PropertyInfo> property_lookup = new Dictionary<string, PropertyInfo>();

                foreach (var property_info in output_type.GetProperties())
                {
                    property_lookup.Add(property_info.Name, property_info);
                }

                while (data_reader.Read())
                {
                    T new_object = new T();

                    for (int i = 0; i < data_reader.FieldCount; i++)
                    {
                        string column_name = data_reader.GetName(i);

                        if (property_lookup.ContainsKey(column_name))
                        {
                            Type property_type = property_lookup[column_name].PropertyType;
                            string property_name = property_lookup[column_name].PropertyType.FullName;
                            var column_value = data_reader[column_name];

                            // in the event that we are looking at a nullable type, we need to look at the underlying type.
                            if (property_type.IsGenericType && property_type.GetGenericTypeDefinition() == typeof(Nullable<>)) 
                                property_name = Nullable.GetUnderlyingType(property_type).ToString();

                            object output_value = new object();

                            switch (property_name)
                            {
                                case "System.Int32":
                                    if (data_reader[i] == DBNull.Value)
                                        output_value = column_value as int? ?? null;
                                    else
                                        output_value = (int)column_value;
                                    break;

                                case "System.String":
                                    if (data_reader[i] == DBNull.Value)
                                        output_value = column_value as int? ?? null;
                                    else
                                        output_value = (string)column_value;
                                    break;

                                case "System.Double":
                                    if (data_reader[i] == DBNull.Value)
                                        output_value = column_value as double? ?? null;
                                    else
                                        output_value = (double)column_value;
                                    break;

                                case "System.Float":
                                    if (data_reader[i] == DBNull.Value)
                                        output_value = column_value as float? ?? null;
                                    else
                                        output_value = (float)column_value;
                                    break;

                                case "System.Boolean":
                                    if (data_reader[i] == DBNull.Value)
                                        output_value = column_value as bool? ?? null;
                                    else
                                        output_value = (bool)column_value;
                                    break;

                                case "System.Boolean[]":
                                    if (data_reader[i] == DBNull.Value)
                                    {
                                        output_value = null;
                                    }
                                    else
                                    {
                                        // inline conversion, blech. improve later.
                                        var byte_array = (byte[])data_reader[i];
                                        var bool_array = new bool[byte_array.Length];

                                        for (int index = 0; index < byte_array.Length; index++)
                                            bool_array[index] = Convert.ToBoolean(byte_array[index]);

                                        output_value = bool_array;
                                    }
                                    break;

                                case "System.DateTime":
                                    if (data_reader[i] == DBNull.Value)
                                        output_value = column_value as DateTime? ?? null;
                                    else
                                        output_value = DateTime.Parse(column_value.ToString());
                                    break;

                                case "System.Guid":
                                    if (data_reader[i] == DBNull.Value)
                                        output_value = column_value as Guid? ?? null;
                                    else
                                        output_value = (Guid)column_value;
                                    break;

                                case "System.Single":
                                    if (data_reader[i] == DBNull.Value)
                                        output_value = column_value as Single? ?? null;
                                    else
                                        output_value = Single.Parse(column_value.ToString());
                                    break;

                                case "System.Decimal":
                                    if (data_reader[i] == DBNull.Value)
                                        output_value = column_value as decimal? ?? null;
                                    else
                                        output_value = (decimal)column_value;
                                    break;

                                case "System.Byte":
                                    if (data_reader[i] == DBNull.Value)
                                        output_value = column_value as byte? ?? null;
                                    else
                                        output_value = (byte)column_value;
                                    break;

                                case "System.Byte[]":
                                    if (data_reader[i] == DBNull.Value)
                                    {
                                        output_value = null;
                                    }
                                    else
                                    {
                                        string byte_array = column_value.ToString();

                                        byte[] bytes = new byte[byte_array.Length * sizeof(char)];
                                        Buffer.BlockCopy(byte_array.ToCharArray(), 0, bytes, 0, bytes.Length);
                                        output_value = bytes;
                                    }
                                    break;

                                case "System.SByte":
                                    if (data_reader[i] == DBNull.Value)
                                        output_value = column_value as sbyte? ?? null;
                                    else
                                        output_value = (sbyte)column_value;
                                    break;

                                case "System.Char":
                                    if (data_reader[i] == DBNull.Value)
                                        output_value = column_value as char? ?? null;
                                    else
                                        output_value = (char)column_value;
                                    break;

                                case "System.UInt32":
                                    if (data_reader[i] == DBNull.Value)
                                        output_value = column_value as uint? ?? null;
                                    else
                                        output_value = (uint)column_value;
                                    break;

                                case "System.Int64":
                                    if (data_reader[i] == DBNull.Value)
                                        output_value = column_value as long? ?? null;
                                    else
                                        output_value = (long)column_value;
                                    break;

                                case "System.UInt64":
                                    if (data_reader[i] == DBNull.Value)
                                        output_value = column_value as ulong? ?? null;
                                    else
                                        output_value = (ulong)column_value;
                                    break;

                                case "System.Object":
                                    if (data_reader[i] == DBNull.Value)
                                        output_value = null;
                                    else
                                        output_value = (object)column_value;
                                    break;

                                case "System.Int16":
                                    if (data_reader[i] == DBNull.Value)
                                        output_value = column_value as short? ?? null;
                                    else
                                        output_value = (short)column_value;
                                    break;

                                case "System.UInt16":
                                    if (data_reader[i] == DBNull.Value)
                                        output_value = column_value as ushort? ?? null;
                                    else
                                        output_value = (ushort)column_value;
                                    break;

                                case "System.Udt":
                                    // no idea how to handle a custom type
                                    throw new Exception("System.Udt is an unsupported datatype");

                                case "Microsoft.SqlServer.Types.SqlGeometry":
                                    if (data_reader[i] == DBNull.Value)
                                        output_value = column_value as Microsoft.SqlServer.Types.SqlGeometry ?? null;
                                    else
                                        output_value = (Microsoft.SqlServer.Types.SqlGeometry)column_value;
                                    break;

                                case "Microsoft.SqlServer.Types.SqlGeography":
                                    if (data_reader[i] == DBNull.Value)
                                        output_value = column_value as Microsoft.SqlServer.Types.SqlGeography ?? null;
                                    else
                                        output_value = (Microsoft.SqlServer.Types.SqlGeography)column_value;
                                    break;

                                default:

                                    if (property_lookup[column_name].PropertyType.IsEnum)
                                    {
                                        // enums are common, but don't fit into the above buckets. 
                                        output_value = Enum.ToObject(property_lookup[column_name].PropertyType,column_value);
                                    }
                                    else
                                    {
                                        throw new Exception(string.Format("Column {0} has an unknown data type: {1}.", property_lookup[column_name], property_lookup[column_name].PropertyType.FullName));
                                    }
                                    break;
                            }

                            property_lookup[column_name].SetValue(new_object, output_value, null);
                        }
                        else
                        {
                            // found a row in data reader that cannot be mapped to a property in object.
                            // might be an error, but it is dependent on the specific use case.
                            if (throw_unmapped_fields_error)
                            {
                                throw new Exception(string.Format("Cannot map datareader field {0} to object property",column_name));
                            }
                        }
                    }

                    results.Add(new_object);
                }

                return results;
            }

        #endregion
    }
}
