using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.NetCore.Tests
{
    [TestClass]
    public class FilesEqual
    {
        private static readonly string TempFolder = @"C:\Wexflow-netcore\Temp\70";

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
            if (!Directory.Exists(TempFolder))
            {
                _ = Directory.CreateDirectory(TempFolder);
            }

            Helper.DeleteFilesAndFolders(TempFolder);
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void FilesEqualTest()
        {
            _ = Helper.StartWorkflow(70);

            // Check the workflow result
            var files = Directory.GetFiles(TempFolder, "FilesEqual*.xml", SearchOption.AllDirectories);
            Assert.AreEqual(files.Length, 1);

            var content = File.ReadAllText(files[0]);
            Assert.AreEqual(ExpectedResult, content);
        }
    }
}