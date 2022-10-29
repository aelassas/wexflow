using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.Tests
{
    [TestClass]
    public class Sha1
    {
        private static readonly string Sha1Folder = @"C:\WexflowTesting\Sha1\";
        private static readonly string ExpectedResult =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n"
            + "<Files>\r\n"
            + "  <File path=\"C:\\WexflowTesting\\Watchfolder1\\file1.txt\" name=\"file1.txt\" sha1=\"8f363337594ece0664b261be156d17fa6842fab2\" />\r\n"
            + "  <File path=\"C:\\WexflowTesting\\Watchfolder1\\file2.txt\" name=\"file2.txt\" sha1=\"8f363337594ece0664b261be156d17fa6842fab2\" />\r\n"
            + "  <File path=\"C:\\WexflowTesting\\Watchfolder1\\file3.mp4\" name=\"file3.mp4\" sha1=\"8f363337594ece0664b261be156d17fa6842fab2\" />\r\n"
            + "  <File path=\"C:\\WexflowTesting\\Watchfolder1\\file3.txt\" name=\"file3.txt\" sha1=\"8f363337594ece0664b261be156d17fa6842fab2\" />\r\n"
            + "</Files>";

        [TestInitialize]
        public void TestInitialize()
        {
            Helper.DeleteFiles(Sha1Folder);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helper.DeleteFiles(Sha1Folder);
        }

        [TestMethod]
        public void Sha1Test()
        {
            string[] files = GetFiles();
            Assert.AreEqual(0, files.Length);
            Helper.StartWorkflow(46);
            files = GetFiles();
            Assert.AreEqual(1, files.Length);
            string content = File.ReadAllText(files[0]);
            Assert.AreEqual(ExpectedResult, content);
        }

        private string[] GetFiles()
        {
            return Directory.GetFiles(Sha1Folder, "SHA1_*.xml");
        }
    }
}
