using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.Tests
{
    [TestClass]
    public class XmlToCsv
    {
        private static readonly string Dest = @"C:\WexflowTesting\XmlToCsv";
        private static readonly string ExpectedResult =
            "content;content;content;\r\n"
            + "content;content;content;\r\n"
            + "content;content;content;\r\n";

        [TestInitialize]
        public void TestInitialize()
        {
            DeleteCsvs();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            DeleteCsvs();
        }

        [TestMethod]
        public void XmlToCsvTest()
        {
            string[] files = GetCsvs();
            Assert.AreEqual(0, files.Length);
            Helper.StartWorkflow(17);
            files = GetCsvs();
            Assert.AreEqual(2, files.Length);
            foreach (var file in files)
            {
                string content = File.ReadAllText(file);
                Assert.AreEqual(ExpectedResult, content);
            }
        }

        private string[] GetCsvs()
        {
            return Directory.GetFiles(Dest, "csv*.csv");
        }

        private void DeleteCsvs()
        {
            string[] files = GetCsvs();
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }
    }
}
