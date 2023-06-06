using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.Tests
{
    [TestClass]
    public class SqlToXml
    {
        private static readonly string SqlToXmlFolder = @"C:\WexflowTesting\SqlToXml\";
        private static readonly string ExpectedResult =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n"
            + "<Records>\r\n"
            + "  <Record>\r\n"
            + "    <Cell column=\"Id\" value=\"1\" />\r\n"
            + "    <Cell column=\"Title\" value=\"Hello World 1!\" />\r\n"
            + "    <Cell column=\"Description\" value=\"Hello World Description 1!\" />\r\n"
            + "  </Record>\r\n"
            + "  <Record>\r\n"
            + "    <Cell column=\"Id\" value=\"2\" />\r\n"
            + "    <Cell column=\"Title\" value=\"Hello World 2!\" />\r\n"
            + "    <Cell column=\"Description\" value=\"Hello World Description 2!\" />\r\n"
            + "  </Record>\r\n"
            + "  <Record>\r\n"
            + "    <Cell column=\"Id\" value=\"3\" />\r\n"
            + "    <Cell column=\"Title\" value=\"Hello World 3!\" />\r\n"
            + "    <Cell column=\"Description\" value=\"Hello World Description 3!\" />\r\n"
            + "  </Record>\r\n"
            + "  <Record>\r\n"
            + "    <Cell column=\"Id\" value=\"4\" />\r\n"
            + "    <Cell column=\"Title\" value=\"Hello World 4!\" />\r\n"
            + "    <Cell column=\"Description\" value=\"Hello World Description 4!\" />\r\n"
            + "  </Record>\r\n"
            + "  <Record>\r\n"
            + "    <Cell column=\"Id\" value=\"5\" />\r\n"
            + "    <Cell column=\"Title\" value=\"Hello World 5!\" />\r\n"
            + "    <Cell column=\"Description\" value=\"Hello World Description 5!\" />\r\n"
            + "  </Record>\r\n"
            + "</Records>";

        [TestInitialize]
        public void TestInitialize()
        {
            Helper.DeleteFiles(SqlToXmlFolder);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helper.DeleteFiles(SqlToXmlFolder);
        }

        [TestMethod]
        public void SqlToXmlTest()
        {
            var files = GetFiles();
            Assert.AreEqual(0, files.Length);
            _ = Helper.StartWorkflow(66);
            files = GetFiles();
            Assert.AreEqual(1, files.Length);
            var content = File.ReadAllText(files[0]);
            Assert.AreEqual(ExpectedResult, content);
        }

        private string[] GetFiles()
        {
            return Directory.GetFiles(SqlToXmlFolder, "SqlToXml_*.xml");
        }
    }
}
