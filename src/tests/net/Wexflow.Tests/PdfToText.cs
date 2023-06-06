using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.Tests
{
    [TestClass]
    public class PdfToText
    {
        private static readonly string PdfToTextFolder = @"C:\WexflowTesting\PdfToText\";

        [TestInitialize]
        public void TestInitialize()
        {
            Helper.DeleteFiles(PdfToTextFolder);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helper.DeleteFiles(PdfToTextFolder);
        }

        [TestMethod]
        public void PdfToTextTest()
        {
            var files = GetFiles();
            Assert.AreEqual(0, files.Length);
            _ = Helper.StartWorkflow(64);
            files = GetFiles();
            Assert.AreEqual(1, files.Length);
        }

        private string[] GetFiles()
        {
            return Directory.GetFiles(PdfToTextFolder, "*.txt");
        }
    }
}
