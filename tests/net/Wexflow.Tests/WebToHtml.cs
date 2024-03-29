using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.Tests
{
    [TestClass]
    public class WebToHtml
    {
        private static readonly string DestDir = @"C:\WexflowTesting\WebToHtml";

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
        public void WebToHtmlTest()
        {
            var files = GetFiles();
            Assert.AreEqual(0, files.Length);
            _ = Helper.StartWorkflow(97);
            files = GetFiles();
            Assert.AreEqual(2, files.Length);
        }

        private string[] GetFiles()
        {
            return Helper.GetFiles(DestDir, "*.html", SearchOption.TopDirectoryOnly);
        }
    }
}
