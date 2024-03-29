using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.Tests
{
    [TestClass]
    public class TextToPdf
    {
        private static readonly string TextToPdfFolder = @"C:\WexflowTesting\TextToPdf\";

        [TestInitialize]
        public void TestInitialize()
        {
            Helper.DeleteFiles(TextToPdfFolder);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helper.DeleteFiles(TextToPdfFolder);
        }

        [TestMethod]
        public void TextToPdfTest()
        {
            var files = GetFiles();
            Assert.AreEqual(0, files.Length);
            _ = Helper.StartWorkflow(64);
            files = GetFiles();
            Assert.AreEqual(1, files.Length);
        }

        private string[] GetFiles()
        {
            return Directory.GetFiles(TextToPdfFolder, "*.pdf");
        }
    }
}
