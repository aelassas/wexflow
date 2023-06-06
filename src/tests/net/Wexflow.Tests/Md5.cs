using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.Tests
{
    [TestClass]
    public class Md5
    {
        private static readonly string Md5Folder = @"C:\WexflowTesting\MD5\";
        private static readonly string ExpectedResult =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
            "<Files>\r\n" +
            "  <File path=\"C:\\WexflowTesting\\Watchfolder1\\file1.txt\" name=\"file1.txt\" md5=\"a3de1332a4235e9a559cfacd9a74a835\" />\r\n" +
            "  <File path=\"C:\\WexflowTesting\\Watchfolder1\\file2.txt\" name=\"file2.txt\" md5=\"a3de1332a4235e9a559cfacd9a74a835\" />\r\n" +
            "  <File path=\"C:\\WexflowTesting\\Watchfolder1\\file3.mp4\" name=\"file3.mp4\" md5=\"a3de1332a4235e9a559cfacd9a74a835\" />\r\n" +
            "  <File path=\"C:\\WexflowTesting\\Watchfolder1\\file3.txt\" name=\"file3.txt\" md5=\"a3de1332a4235e9a559cfacd9a74a835\" />\r\n" +
            "</Files>";

        [TestInitialize]
        public void TestInitialize()
        {
            Helper.DeleteFiles(Md5Folder);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helper.DeleteFiles(Md5Folder);
        }

        [TestMethod]
        public void Md5Test()
        {
            var files = GetFiles();
            Assert.AreEqual(0, files.Length);
            _ = Helper.StartWorkflow(10);
            files = GetFiles();
            Assert.AreEqual(1, files.Length);
            var content = File.ReadAllText(files[0]);
            Assert.AreEqual(ExpectedResult, content);
        }

        private string[] GetFiles()
        {
            return Directory.GetFiles(Md5Folder, "MD5_*.xml");
        }
    }
}
