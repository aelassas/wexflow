using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.NetCore.Tests
{
    [TestClass]
    public class Ftp
    {
        private static readonly string Temp = @"C:\Wexflow-netcore\Temp\55";
        private static readonly string FtpDownload = @"C:\WexflowTesting\Ftp_download";
        private static readonly string File1 = @"C:\WexflowTesting\Ftp_download\file1.txt";
        private static readonly string File2 = @"C:\WexflowTesting\Ftp_download\file2.txt";

        private static readonly string ExpectedResult =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n"
            + "<WexflowProcessing>\r\n"
            + "  <Workflow id=\"55\" name=\"Workflow_Ftp\" description=\"Workflow_Ftp\">\r\n"
            + "    <Files>\r\n"
            + "      <File taskId=\"99\" path=\"C:\\WexflowTesting\\file3.txt\" name=\"file3.txt\" renameTo=\"\" renameToOrName=\"file3.txt\" />\r\n"
            + "      <File taskId=\"99\" path=\"C:\\WexflowTesting\\file4.txt\" name=\"file4.txt\" renameTo=\"\" renameToOrName=\"file4.txt\" />\r\n"
            + "      <File taskId=\"3\" path=\"C:\\WexflowTesting\\file1.txt\" name=\"file1.txt\" renameTo=\"\" renameToOrName=\"file1.txt\" />\r\n"
            + "      <File taskId=\"3\" path=\"C:\\WexflowTesting\\file2.txt\" name=\"file2.txt\" renameTo=\"\" renameToOrName=\"file2.txt\" />\r\n"
            + "      <File taskId=\"5\" path=\"/file1.txt\" name=\"file1.txt\" renameTo=\"\" renameToOrName=\"file1.txt\" />\r\n"
            + "      <File taskId=\"5\" path=\"/file2.txt\" name=\"file2.txt\" renameTo=\"\" renameToOrName=\"file2.txt\" />\r\n"
            + "    </Files>\r\n"
            + "  </Workflow>\r\n"
            + "</WexflowProcessing>";

        [TestInitialize]
        public void TestInitialize()
        {
            if (!Directory.Exists(Temp))
            {
                _ = Directory.CreateDirectory(Temp);
            }

            Helper.DeleteFilesAndFolders(Temp);
            Helper.DeleteFiles(FtpDownload);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helper.DeleteFiles(FtpDownload);
        }

        [TestMethod]
        public void FtpTest()
        {
            Assert.IsFalse(File.Exists(File1));
            Assert.IsFalse(File.Exists(File2));
            var files = GetFiles();
            Assert.AreEqual(0, files.Length);
            _ = Helper.StartWorkflow(55); // list+upload+download+delete
            files = GetFiles();
            Assert.AreEqual(1, files.Length);
            var content = File.ReadAllText(files[0]);
            Assert.AreEqual(ExpectedResult, content);
            Assert.IsTrue(File.Exists(File1));
            Assert.IsTrue(File.Exists(File2));
            // TODO sftp and ftps
        }

        private static string[] GetFiles()
        {
            return Directory.GetFiles(Temp, "ListFiles_*.xml", SearchOption.AllDirectories);
        }
    }
}
