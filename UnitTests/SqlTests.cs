using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestFixture]
    public class SqlTests
    {
        [SetUp]
        public void InitTest()
        {

        }

        [TearDown]
        public void TeardownTest()
        {

        }

        [Test]
        public void t1()
        {
            //Assert.IsTrue(!string.IsNullOrWhiteSpace(hash1));
        }

        [Test]
        public void t2()
        {
            //Assert.Throws(typeof(ArgumentException), () => new CryptoHashGenerator(DEFAULT_ITERATIONS, DEFAULT_HASH_SIZE, 0));
        }
    }

}
