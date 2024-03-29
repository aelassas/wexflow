using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.NetCore.Tests
{
    [TestClass]
    public class FilesLoader
    {
        private static readonly string TempFolder = @"C:\Wexflow-netcore\Temp\3";

        private static readonly string ExpectedResult =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
            "<WexflowProcessing>\r\n" +
            "  <Workflow id=\"3\" name=\"Workflow_FilesLoader\" description=\"Workflow_FilesLoader\">\r\n" +
            "    <Files>\r\n" +
            "      <File taskId=\"1\" path=\"C:\\WexflowTesting\\Watchfolder1\\file3.mp4\" name=\"file3.mp4\" renameTo=\"\" renameToOrName=\"file3.mp4\" />\r\n" +
            "      <File taskId=\"1\" path=\"C:\\WexflowTesting\\Watchfolder1\\Watchfolder2\\file1.mp4\" name=\"file1.mp4\" renameTo=\"\" renameToOrName=\"file1.mp4\" />\r\n" +
            "      <File taskId=\"1\" path=\"C:\\WexflowTesting\\Watchfolder1\\Watchfolder2\\Watchfolder3\\file2.mp4\" name=\"file2.mp4\" renameTo=\"\" renameToOrName=\"file2.mp4\" />\r\n" +
            "      <File taskId=\"1\" path=\"C:\\WexflowTesting\\file1.txt\" name=\"file1.txt\" renameTo=\"\" renameToOrName=\"file1.txt\" />\r\n" +
            "      <File taskId=\"1\" path=\"C:\\WexflowTesting\\file2.txt\" name=\"file2.txt\" renameTo=\"\" renameToOrName=\"file2.txt\" />\r\n" +
            "      <File taskId=\"1\" path=\"C:\\WexflowTesting\\file3.txt\" name=\"file3.txt\" renameTo=\"\" renameToOrName=\"file3.txt\" />\r\n" +
            "      <File taskId=\"1\" path=\"C:\\WexflowTesting\\file4.txt\" name=\"file4.txt\" renameTo=\"\" renameToOrName=\"file4.txt\" />\r\n" +
            "      <File taskId=\"1\" path=\"C:\\WexflowTesting\\file5.txt\" name=\"file5.txt\" renameTo=\"\" renameToOrName=\"file5.txt\" />\r\n" +
            "    </Files>\r\n" +
            "  </Workflow>\r\n" +
            "</WexflowProcessing>";

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
        public void FilesLoaderTest()
        {
            _ = Helper.StartWorkflow(3);

            // Check the workflow result
            var files = Directory.GetFiles(TempFolder, "ListFiles*.xml", SearchOption.AllDirectories);
            Assert.AreEqual(1, files.Length);

            var content = File.ReadAllText(files[0]);
            Assert.AreEqual(ExpectedResult, content);
        }
    }
}
