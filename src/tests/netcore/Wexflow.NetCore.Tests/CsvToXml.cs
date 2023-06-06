using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.NetCore.Tests
{
    [TestClass]
    public class CsvToXml
    {
        private static readonly string ExpectedResult =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
            "<Lines>\r\n" +
            "  <Line>\r\n" +
            "    <Column>content</Column>\r\n" +
            "    <Column>content</Column>\r\n" +
            "    <Column>content</Column>\r\n" +
            "  </Line>\r\n" +
            "  <Line>\r\n" +
            "    <Column>content</Column>\r\n" +
            "    <Column>content</Column>\r\n" +
            "    <Column>content</Column>\r\n" +
            "  </Line>\r\n" +
            "  <Line>\r\n" +
            "    <Column>content</Column>\r\n" +
            "    <Column>content</Column>\r\n" +
            "    <Column>content</Column>\r\n" +
            "  </Line>\r\n" +
            "</Lines>";

        [TestInitialize]
        public void TestInitialize()
        {
            DeleteXmls();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            DeleteXmls();
        }

        [TestMethod]
        public void CsvToXmlTest()
        {
            _ = Helper.StartWorkflow(1);

            // Check the workflow result
            var xmlFiles = Directory.GetFiles(@"C:\WexflowTesting\CsvToXml\", "*.xml");
            Assert.AreEqual(2, xmlFiles.Length);

            foreach (var xmlFile in xmlFiles)
            {
                var xmlContent = File.ReadAllText(xmlFile);
                Assert.AreEqual(ExpectedResult, xmlContent);
            }
        }

        private static void DeleteXmls()
        {
            foreach (var file in Directory.GetFiles(@"C:\WexflowTesting\CsvToXml\", "*.xml"))
            {
                File.Delete(file);
            }
        }
    }
}
