using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.Tests
{
    [TestClass]
    public class Sync
    {
        private static readonly string Temp = @"C:\Wexflow\Temp\26";
        private static readonly string Src = @"C:\WexflowTesting\Sync_src";
        private static readonly string Dest = @"C:\WexflowTesting\Sync_dest";
        private static readonly string ExpectedResult =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
            "<WexflowProcessing>\r\n" +
            "  <Workflow id=\"26\" name=\"Workflow_Sync\" description=\"Workflow_Sync\">\r\n" +
            "    <Files>\r\n" +
            "      <File taskId=\"2\" path=\"C:\\WexflowTesting\\Sync_dest\\file1.txt\" name=\"file1.txt\" renameTo=\"\" renameToOrName=\"file1.txt\" />\r\n" +
            "      <File taskId=\"2\" path=\"C:\\WexflowTesting\\Sync_dest\\file2.txt\" name=\"file2.txt\" renameTo=\"\" renameToOrName=\"file2.txt\" />\r\n" +
            "      <File taskId=\"2\" path=\"C:\\WexflowTesting\\Sync_dest\\file3.txt\" name=\"file3.txt\" renameTo=\"\" renameToOrName=\"file3.txt\" />\r\n" +
            "      <File taskId=\"2\" path=\"C:\\WexflowTesting\\Sync_dest\\filesync.id\" name=\"filesync.id\" renameTo=\"\" renameToOrName=\"filesync.id\" />\r\n" +
            "      <File taskId=\"2\" path=\"C:\\WexflowTesting\\Sync_dest\\filesync.metadata\" name=\"filesync.metadata\" renameTo=\"\" renameToOrName=\"filesync.metadata\" />\r\n" +
            "      <File taskId=\"2\" path=\"C:\\WexflowTesting\\Sync_dest\\folder1\\file1.txt\" name=\"file1.txt\" renameTo=\"\" renameToOrName=\"file1.txt\" />\r\n" +
            "      <File taskId=\"2\" path=\"C:\\WexflowTesting\\Sync_dest\\folder1\\file2.txt\" name=\"file2.txt\" renameTo=\"\" renameToOrName=\"file2.txt\" />\r\n" +
            "      <File taskId=\"2\" path=\"C:\\WexflowTesting\\Sync_dest\\folder1\\file3.txt\" name=\"file3.txt\" renameTo=\"\" renameToOrName=\"file3.txt\" />\r\n" +
            "      <File taskId=\"2\" path=\"C:\\WexflowTesting\\Sync_dest\\folder1\\subfolder1\\file1.txt\" name=\"file1.txt\" renameTo=\"\" renameToOrName=\"file1.txt\" />\r\n" +
            "      <File taskId=\"2\" path=\"C:\\WexflowTesting\\Sync_dest\\folder1\\subfolder1\\file2.txt\" name=\"file2.txt\" renameTo=\"\" renameToOrName=\"file2.txt\" />\r\n" +
            "      <File taskId=\"2\" path=\"C:\\WexflowTesting\\Sync_dest\\folder1\\subfolder1\\file3.txt\" name=\"file3.txt\" renameTo=\"\" renameToOrName=\"file3.txt\" />\r\n" +
            "    </Files>\r\n" +
            "  </Workflow>\r\n" +
            "</WexflowProcessing>";

        [TestInitialize]
        public void TestInitialize()
        {
            if (!Directory.Exists(Temp)) Directory.CreateDirectory(Temp);
            Helper.DeleteFilesAndFolders(Temp);
            Helper.DeleteFilesAndFolders(Dest);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helper.DeleteFilesAndFolders(Dest);
        }

        [TestMethod]
        public void SyncTest()
        {
            Assert.AreEqual(true, Directory.Exists(Src));
            Assert.AreEqual(true, Directory.Exists(Dest));
            string[] files = GetFiles();
            Assert.AreEqual(0, files.Length);
            Helper.StartWorkflow(26);
            files = GetFiles();
            Assert.AreEqual(1, files.Length);
            string content = File.ReadAllText(files[0]);
            Assert.AreEqual(ExpectedResult, content);
        }

        private string[] GetFiles()
        {
            return Directory.GetFiles(Temp, "ListFiles*.xml", SearchOption.AllDirectories);
        }
    }
}
