using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.NetCore.Tests
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
            var files = GetCsvs();
            Assert.AreEqual(0, files.Length);
            _ = Helper.StartWorkflow(17);
            files = GetCsvs();
            Assert.AreEqual(2, files.Length);
            foreach (var file in files)
            {
                var content = File.ReadAllText(file);
                Assert.AreEqual(ExpectedResult, content);
            }
        }

        private static string[] GetCsvs()
        {
            return Directory.GetFiles(Dest, "csv*.csv");
        }

        private static void DeleteCsvs()
        {
            var files = GetCsvs();
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }
    }
}
