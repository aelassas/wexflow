using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.Tests
{
    [TestClass]
    public class ListFiles
    {
        private static readonly string Temp = @"C:\Wexflow\Temp\8";
        private static readonly string ExpectedResult =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
            "<WexflowProcessing>\r\n" +
            "  <Workflow id=\"8\" name=\"Workflow_ListFiles\" description=\"Workflow_ListFiles\">\r\n" +
            "    <Files>\r\n" +
            "      <File taskId=\"1\" path=\"C:\\WexflowTesting\\file1.txt\" name=\"file1.txt\" renameTo=\"\" renameToOrName=\"file1.txt\" />\r\n" +
            "      <File taskId=\"1\" path=\"C:\\WexflowTesting\\file2.txt\" name=\"file2.txt\" renameTo=\"\" renameToOrName=\"file2.txt\" />\r\n" +
            "      <File taskId=\"1\" path=\"C:\\WexflowTesting\\file3.txt\" name=\"file3.txt\" renameTo=\"\" renameToOrName=\"file3.txt\" />\r\n" +
            "      <File taskId=\"1\" path=\"C:\\WexflowTesting\\file4.txt\" name=\"file4.txt\" renameTo=\"\" renameToOrName=\"file4.txt\" />\r\n" +
            "    </Files>\r\n" +
            "  </Workflow>\r\n" +
            "</WexflowProcessing>";

        [TestInitialize]
        public void TestInitialize()
        {
            if (!Directory.Exists(Temp))
            {
                _ = Directory.CreateDirectory(Temp);
            }

            Helper.DeleteFilesAndFolders(Temp);
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void ListFilesTest()
        {
            var files = GetFiles();
            Assert.AreEqual(0, files.Length);
            _ = Helper.StartWorkflow(8);
            files = GetFiles();
            Assert.AreEqual(1, files.Length);
            var content = File.ReadAllText(files[0]);
            Assert.AreEqual(ExpectedResult, content);
        }

        private string[] GetFiles()
        {
            return Directory.GetFiles(Temp, "ListFiles*.xml", SearchOption.AllDirectories);
        }
    }
}
