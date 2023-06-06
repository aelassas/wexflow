using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.Tests
{
    [TestClass]
    public class Xslt
    {
        private static readonly string DestDir = @"C:\WexflowTesting\Xslt\";
        private static readonly string ExpectedResult =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n"
            + "<FinalProducts>\r\n"
            + "  <FinalProduct name=\"Product1\" description=\"Product1 description\" />\r\n"
            + "  <FinalProduct name=\"Product2\" description=\"Product2 description\" />\r\n"
            + "  <FinalProduct name=\"Product3\" description=\"Product3 description\" />\r\n"
            + "  <FinalProduct name=\"Product4\" description=\"Product4 description\" />\r\n"
            + "</FinalProducts>";

        [TestInitialize]
        public void TestInitialize()
        {
            Helper.DeleteFiles(DestDir);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helper.DeleteFiles(DestDir);
        }

        [TestMethod]
        public void XsltTest()
        {
            var files = GetFiles();
            Assert.AreEqual(0, files.Length);
            _ = Helper.StartWorkflow(18);
            files = GetFiles();
            Assert.AreEqual(1, files.Length);
            var content = File.ReadAllText(files[0]);
            Assert.AreEqual(ExpectedResult, content);
        }

        private string[] GetFiles()
        {
            return Directory.GetFiles(DestDir, "*.xml");
        }
    }
}