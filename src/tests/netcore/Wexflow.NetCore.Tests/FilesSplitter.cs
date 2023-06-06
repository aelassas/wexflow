using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.NetCore.Tests
{
    [TestClass]
    public class FilesSplitter
    {
        private static readonly string FilesSplitterFolder = @"C:\WexflowTesting\FilesSplitter\";

        [TestInitialize]
        public void TestInitialize()
        {
            Helper.DeleteFiles(FilesSplitterFolder);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helper.DeleteFiles(FilesSplitterFolder);
        }

        [TestMethod]
        public void FilesSplitterTest()
        {
            var files = GetFiles();
            Assert.AreEqual(0, files.Length);
            _ = Helper.StartWorkflow(57);
            files = GetFiles();
            Assert.AreEqual(510, files.Length);
        }

        private static string[] GetFiles()
        {
            return Directory.GetFiles(FilesSplitterFolder, "*_*");
        }
    }
}
