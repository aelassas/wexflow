using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.Tests
{
    [TestClass]
    public class SqlToCsv
    {
        private static readonly string SqlToCsvFolder = @"C:\WexflowTesting\SqlToCsv\";
        private static readonly string ExpectedResult =
            "Id;Title;Description\r\n"
            + "1;Hello World 1!;Hello World Description 1!\r\n"
            + "2;Hello World 2!;Hello World Description 2!\r\n"
            + "3;Hello World 3!;Hello World Description 3!\r\n"
            + "4;Hello World 4!;Hello World Description 4!\r\n"
            + "5;Hello World 5!;Hello World Description 5!\r\n";

        [TestInitialize]
        public void TestInitialize()
        {
            Helper.DeleteFiles(SqlToCsvFolder);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helper.DeleteFiles(SqlToCsvFolder);
        }

        [TestMethod]
        public void SqlToCsvTest()
        {
            var files = GetFiles();
            Assert.AreEqual(0, files.Length);
            _ = Helper.StartWorkflow(67);
            files = GetFiles();
            Assert.AreEqual(1, files.Length);
            var content = File.ReadAllText(files[0]);
            Assert.AreEqual(ExpectedResult, content);
        }

        private string[] GetFiles()
        {
            return Directory.GetFiles(SqlToCsvFolder, "SqlToCsv_*.csv");
        }
    }
}
