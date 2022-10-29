using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            string[] files = GetFiles();
            Assert.AreEqual(0, files.Length);
            Helper.StartWorkflow(65);
            files = GetFiles();
            Assert.AreEqual(1, files.Length);
        }

        private string[] GetFiles()
        {
            return Directory.GetFiles(HtmlToPdfFolder, "*.pdf");
        }
    }
}
