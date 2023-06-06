using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.NetCore.Tests
{
    [TestClass]
    public class Sha256
    {
        private static readonly string Sha256Folder = @"C:\WexflowTesting\Sha256\";
        private static readonly string ExpectedResult =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n"
            + "<Files>\r\n"
            + "  <File path=\"C:\\WexflowTesting\\Watchfolder1\\file1.txt\" name=\"file1.txt\" sha256=\"ddd23773d8724b8a40749046d8e3414402507a21a1ccc522653197b0803d268b\" />\r\n"
            + "  <File path=\"C:\\WexflowTesting\\Watchfolder1\\file2.txt\" name=\"file2.txt\" sha256=\"ddd23773d8724b8a40749046d8e3414402507a21a1ccc522653197b0803d268b\" />\r\n"
            + "  <File path=\"C:\\WexflowTesting\\Watchfolder1\\file3.mp4\" name=\"file3.mp4\" sha256=\"ddd23773d8724b8a40749046d8e3414402507a21a1ccc522653197b0803d268b\" />\r\n"
            + "  <File path=\"C:\\WexflowTesting\\Watchfolder1\\file3.txt\" name=\"file3.txt\" sha256=\"ddd23773d8724b8a40749046d8e3414402507a21a1ccc522653197b0803d268b\" />\r\n"
            + "</Files>";

        [TestInitialize]
        public void TestInitialize()
        {
            Helper.DeleteFiles(Sha256Folder);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helper.DeleteFiles(Sha256Folder);
        }

        [TestMethod]
        public void Sha256Test()
        {
            var files = GetFiles();
            Assert.AreEqual(0, files.Length);
            _ = Helper.StartWorkflow(47);
            files = GetFiles();
            Assert.AreEqual(1, files.Length);
            var content = File.ReadAllText(files[0]);
            Assert.AreEqual(ExpectedResult, content);
        }

        private static string[] GetFiles()
        {
            return Directory.GetFiles(Sha256Folder, "SHA256_*.xml");
        }
    }
}
