using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.Tests
{
    [TestClass]
    public class FilesInfo
    {
        private static readonly string FilesInfoFolder = @"C:\WexflowTesting\FilesInfo\";
        private static readonly string File1 = @"C:\WexflowTesting\file1.txt";
        private static readonly string File2 = @"C:\WexflowTesting\file2.txt";

        private static string _expectedResult =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n"
            + "<Files>\r\n"
            + "  <File path=\"C:\\WexflowTesting\\file1.txt\" name=\"file1.txt\" renameToOrName=\"file1.txt\" createdOn=\"{0}\" lastWriteOn=\"{1}\" lastAcessOn=\"{2}\" isReadOnly=\"false\" length=\"5066\" attributes=\"Archive, NotContentIndexed\" />\r\n"
            + "  <File path=\"C:\\WexflowTesting\\file2.txt\" name=\"file2.txt\" renameToOrName=\"file2.txt\" createdOn=\"{3}\" lastWriteOn=\"{4}\" lastAcessOn=\"{5}\" isReadOnly=\"false\" length=\"27\" attributes=\"Archive, NotContentIndexed\" />\r\n"
            + "</Files>";

        [TestInitialize]
        public void TestInitialize()
        {
            const string dateFormat = @"MM\/dd\/yyyy HH:mm.ss";
            Helper.DeleteFiles(FilesInfoFolder);
            var info1 = new FileInfo(File1);
            var info2 = new FileInfo(File2);
            _expectedResult = string.Format(_expectedResult
                , info1.CreationTime.ToString(dateFormat)
                , info1.LastWriteTime.ToString(dateFormat)
                , info1.LastAccessTime.ToString(dateFormat)
                , info2.CreationTime.ToString(dateFormat)
                , info2.LastWriteTime.ToString(dateFormat)
                , info2.LastAccessTime.ToString(dateFormat));
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helper.DeleteFiles(FilesInfoFolder);
        }

        [TestMethod]
        public void FilesInfoTest()
        {
            var files = GetFiles();
            Assert.AreEqual(0, files.Length);
            _ = Helper.StartWorkflow(54);
            files = GetFiles();
            Assert.AreEqual(1, files.Length);
            var content = File.ReadAllText(files[0]);
            Assert.AreEqual(_expectedResult, content);
        }

        private string[] GetFiles()
        {
            return Directory.GetFiles(FilesInfoFolder, "FilesInfo_*.xml");
        }
    }
}
