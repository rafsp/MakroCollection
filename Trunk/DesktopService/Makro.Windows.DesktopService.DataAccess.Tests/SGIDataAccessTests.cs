using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Data.Common;

namespace Makro.Windows.DesktopService.DataAccess.Tests
{
    /// <summary>
    /// Summary description for SGIDataAccessTests
    /// </summary>
    [TestClass]
    public class SGIDataAccessTests
    {
        public SGIDataAccessTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void SGIDataAccess_Ctor()
        {

            var sgida = new SGIDataAccess(null);

            try
            {
                var sgida1 = new SGIDataAccess(null, null);
                Assert.Fail("Exception not thrown for null DbProviderFactory");
            }
            catch (Exception e)
            {
                Assert.IsInstanceOfType(e, typeof(NullReferenceException));
            }

            try
            {
                var sgida1 = new SGIDataAccess(new MockDbProviderFactory(), null);
                Assert.Fail("Exception not thrown for null DbProviderFactory");
            }
            catch (Exception e)
            {
                Assert.IsInstanceOfType(e, typeof(NullReferenceException));
            }

            try
            {
                var sgida1 = new SGIDataAccess(new MockDbProviderFactory(), "Test");
                Assert.Fail("Exception not thrown for null DbProviderFactory");
            }
            catch (Exception e) {
                Assert.IsInstanceOfType(e, typeof(NullReferenceException));
            }
        }

        [TestMethod]
        public void SGIDataAccess_Connection()
        {
            var sgida = new SGIDataAccess(null);
            sgida.Connection = null;
            Assert.IsNull(sgida.Connection);
        }

        [TestMethod]
        public void SGIDataAccess_IsUserLockable()
        {
            var sgida = new SGIDataAccess(null);
            sgida.IsUserLockable(null);
        }

        [TestMethod]
        public void SGIDataAccess_Dispose()
        {
            using (var sgida = new SGIDataAccess(null))
            {
            }
        }

        public class MockDbConnection : IDbConnection
        {

            public IDbTransaction BeginTransaction(IsolationLevel il)
            {
                return null;
            }

            public IDbTransaction BeginTransaction()
            {
                return null;
            }

            public void ChangeDatabase(string databaseName)
            {
            }

            public void Close()
            {
            }

            public string ConnectionString
            {
                get;
                set;
            }

            int _connectionTimeout;
            public int ConnectionTimeout
            {
                get { return _connectionTimeout; }
            }

            public IDbCommand CreateCommand()
            {
                return null;
            }

            string _database;
            public string Database
            {
                get { return _database; } 
            }

            public void Open()
            {
            }

            ConnectionState _state;
            public ConnectionState State
            {
                get { return _state; }
            }

            public void Dispose()
            {
            }
        }
        public class MockDbProviderFactory : DbProviderFactory
        {
        }
    }
}
