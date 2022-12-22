using DAL.Standard.SqlMetadata;
using NUnit.Framework;
using System.Collections.Generic;

namespace UnitTests
{
    [TestFixture]
    public class SqlTests
    {
        SqlTable _SqlTable;

        [SetUp]
        public void InitTest()
        {
            var tableName = "testDb";

            _SqlTable = new SqlTable()
            {
                Name = tableName,
            };

            SqlColumn column;

            // int PK + ID
            column = new SqlColumn
            {
                ColumnOrdinal = 1,
                DataType = "Int",
                DefaultValue = string.Empty,
                IsIdentity = true,
                IsNullable = false,
                IsPk = true,
                Length = 4,
                Name = "Id",
                Precision = 10,
                Scale = 0,
                Schema = "dbo",
                Table = _SqlTable
            };
            _SqlTable.Columns.Add(column.Name, column);

            column = new SqlColumn
            {
                ColumnOrdinal = 1,
                DataType = "Varchar",
                DefaultValue = string.Empty,
                IsIdentity = false,
                IsNullable = true,
                IsPk = false,
                Length = 50,
                Name = "Name",
                Precision = 0,
                Scale = 0,
                Schema = "dbo",
                Table = _SqlTable
            };
            _SqlTable.Columns.Add(column.Name, column);

            column = new SqlColumn
            {
                ColumnOrdinal = 1,
                DataType = "Varchar",
                DefaultValue = string.Empty,
                IsIdentity = false,
                IsNullable = true,
                IsPk = false,
                Length = 50,
                Name = "Address",
                Precision = 0,
                Scale = 0,
                Schema = "dbo",
                Table = _SqlTable
            };
            _SqlTable.Columns.Add(column.Name, column);

            column = new SqlColumn
            {
                ColumnOrdinal = 1,
                DataType = "Int",
                DefaultValue = "21",
                IsIdentity = false,
                IsNullable = false,
                IsPk = false,
                Length = 4,
                Name = "Age",
                Precision = 10,
                Scale = 0,
                Schema = "dbo",
                Table = _SqlTable
            };
            _SqlTable.Columns.Add(column.Name, column);

            _SqlTable.Database = new SqlDatabase()
            {
                ConnectionString = "Server=myServerAddress;Database=myDataBase;Trusted_Connection=True;",
                Tables = new Dictionary<string, SqlTable>()
                {
                    { tableName, _SqlTable }
                },
                Name = tableName,
                StoredProcedures = null,
                Constraints = null,
                Functions = null,
            };
        }

        [TearDown]
        public void TeardownTest()
        {

        }

        [Test]
        public void t1()
        {
            // todo: create fake sqldb object to test with

            //var foo = new CodeGeneratorCSharp();
            //var data = new AutogenerationData()
            //{
            //    NamespaceIncludes = new List<string>(),
            //    Options = new Dictionary<string, bool>(),
            //    SqlTable = _SqlTable,
            //};

            //var output = foo.GenerateCSharpClassInterface(data);
            //Assert.IsTrue(output != null);
        }

        [Test]
        public void t2()
        {
            //Assert.Throws(typeof(ArgumentException), () => new CryptoHashGenerator(DEFAULT_ITERATIONS, DEFAULT_HASH_SIZE, 0));
        }
    }

}
