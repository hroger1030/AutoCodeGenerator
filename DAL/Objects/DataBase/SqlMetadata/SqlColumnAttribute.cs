using System;
using System.Data;

namespace DAL.SqlMetadata
{
    public class SqlColumnAttribute : Attribute 
    {
        public string ColumnName { get;set; }

        public SqlDbType SqlType { get;set; }

        public int Length { get;set; }

        public bool IsPrimaryKey { get;set; }

        public bool IsNullable { get;set; }
    }
}
