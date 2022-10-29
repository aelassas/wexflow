using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.Tests
{
    [TestClass]
    public class FilesEqual
    {
        private static readonly string TempFolder = @"C:\Wexflow\Temp\70";

        private static readonly string ExpectedResult =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n"
            + "<Root>\r\n"
            + "  <Files>\r\n"
            + "    <File path=\"C:\\WexflowTesting\\file1.txt\" name=\"file1.txt\" />\r\n"
            + "    <File path=\"C:\\WexflowTesting\\file2.txt\" name=\"file2.txt\" />\r\n"
            + "  </Files>\r\n"
            + "  <Result>false</Result>\r\n"
            + "</Root>";

        [TestInitialize]
        public void TestInitialize()
        {
            if (!Directory.Exists(TempFolder)) Directory.CreateDirectory(TempFolder);
            Helper.DeleteFilesAndFolders(TempFolder);
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void FilesEqualTest()
        {
            Helper.StartWorkflow(70);

            // Check the workflow result
            string[] files = Directory.GetFiles(TempFolder, "FilesEqual*.xml", SearchOption.AllDirectories);
            Assert.AreEqual(files.Length, 1);

            string content = File.ReadAllText(files[0]);
            Assert.AreEqual(ExpectedResult, content);
        }
    }
}