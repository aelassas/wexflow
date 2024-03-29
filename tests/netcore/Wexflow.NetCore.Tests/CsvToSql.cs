using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.NetCore.Tests
{
    [TestClass]
    public class CsvToSql
    {
        private static readonly string Dest = @"C:\WexflowTesting\CsvToSql";
        private static readonly string ExpectedResult =
            "INSERT INTO HelloWorld(Id,Title,Description) VALUES (1, 'Hello World 1!', 'Hello World Description 1!');\r\n"
          + "INSERT INTO HelloWorld(Id,Title,Description) VALUES (2, 'Hello World 2!', 'Hello World Description 2!');\r\n"
          + "INSERT INTO HelloWorld(Id,Title,Description) VALUES (3, 'Hello World 3!', 'Hello World Description 3!');\r\n"
          + "INSERT INTO HelloWorld(Id,Title,Description) VALUES (4, 'Hello World 4!', 'Hello World Description 4!');\r\n"
          + "INSERT INTO HelloWorld(Id,Title,Description) VALUES (5, 'Hello World 5!', 'Hello World Description 5!');\r\n";

        [TestInitialize]
        public void TestInitialize()
        {
            DeleteSqlScripts();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            DeleteSqlScripts();
        }

        [TestMethod]
        public void CsvToSqlTest()
        {
            var files = GetSqlScripts();
            Assert.AreEqual(0, files.Length);
            _ = Helper.StartWorkflow(76);
            files = GetSqlScripts();
            Assert.AreEqual(2, files.Length);
            foreach (var file in files)
            {
                var content = File.ReadAllText(file);
                Assert.AreEqual(ExpectedResult, content);
            }
        }

        private static string[] GetSqlScripts()
        {
            return Directory.GetFiles(Dest, "csv*.sql");
        }

        private static void DeleteSqlScripts()
        {
            var files = GetSqlScripts();
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }
    }
}
