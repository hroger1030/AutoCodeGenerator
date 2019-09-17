using System.Collections.Generic;

using AutoCodeGenLibrary;
using DAL.Framework.SqlMetadata;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class SqlTests
    {
        SqlTable _SqlTable;

        [SetUp]
        public void InitTest()
        {
            _SqlTable = new SqlTable();
            SqlColumn column;

            // int PK + ID
            column = new SqlColumn();
            column.ColumnOrdinal = 1;
            column.DataType = "Int";
            column.DefaultValue = string.Empty;
            column.IsIdentity = true;
            column.IsNullable = false;
            column.IsPk = true;
            column.Length = 4;
            column.Name = "Id";
            column.Precision = 10;
            column.Scale = 0;
            column.Schema = "dbo";
            column.Table = _SqlTable;
            _SqlTable.Columns.Add(column.Name, column);

            column = new SqlColumn();
            column.ColumnOrdinal = 1;
            column.DataType = "Varchar";
            column.DefaultValue = string.Empty;
            column.IsIdentity = false;
            column.IsNullable = true;
            column.IsPk = false;
            column.Length = 50;
            column.Name = "Name";
            column.Precision = 0;
            column.Scale = 0;
            column.Schema = "dbo";
            column.Table = _SqlTable;
            _SqlTable.Columns.Add(column.Name, column);

            column = new SqlColumn();
            column.ColumnOrdinal = 1;
            column.DataType = "Varchar";
            column.DefaultValue = string.Empty;
            column.IsIdentity = false;
            column.IsNullable = true;
            column.IsPk = false;
            column.Length = 50;
            column.Name = "Address";
            column.Precision = 0;
            column.Scale = 0;
            column.Schema = "dbo";
            column.Table = _SqlTable;
            _SqlTable.Columns.Add(column.Name, column);

            column = new SqlColumn();
            column.ColumnOrdinal = 1;
            column.DataType = "Int";
            column.DefaultValue = "21";
            column.IsIdentity = false;
            column.IsNullable = false;
            column.IsPk = false;
            column.Length = 4;
            column.Name = "Age";
            column.Precision = 10;
            column.Scale = 0;
            column.Schema = "dbo";
            column.Table = _SqlTable;
            _SqlTable.Columns.Add(column.Name, column);
        }

        [TearDown]
        public void TeardownTest()
        {

        }

        [Test]
        public void t1()
        {
            var foo = new CodeGenerator();


            var output = foo.GenerateCSharpClassInterface(_SqlTable, new List<string>(), false);
            Assert.IsTrue(output != null);
        }

        [Test]
        public void t2()
        {
            //Assert.Throws(typeof(ArgumentException), () => new CryptoHashGenerator(DEFAULT_ITERATIONS, DEFAULT_HASH_SIZE, 0));
        }
    }

}
