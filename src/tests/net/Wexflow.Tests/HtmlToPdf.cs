using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.Tests
{
    [TestClass]
    public class HtmlToPdf
    {
        private static readonly string HtmlToPdfFolder = @"C:\WexflowTesting\HtmlToPdf\";

        [TestInitialize]
        public void TestInitialize()
        {
            Helper.DeleteFiles(HtmlToPdfFolder);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helper.DeleteFiles(HtmlToPdfFolder);
        }

        //[Ignore("There is an issue whith TuesPechkin and MSTest but it works fine on prod.")]
        [TestMethod]
        public void HtmlToPdfTest()
        {
            var files = GetFiles();
            Assert.AreEqual(0, files.Length);
            _ = Helper.StartWorkflow(65);
            files = GetFiles();
            Assert.AreEqual(1, files.Length);
        }

        private string[] GetFiles()
        {
            return Directory.GetFiles(HtmlToPdfFolder, "*.pdf");
        }
    }
}
