using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.NetCore.Tests
{
    [TestClass]
    public class Sha512
    {
        private static readonly string Sha512Folder = @"C:\WexflowTesting\Sha512\";
        private static readonly string ExpectedResult =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n"
            + "<Files>\r\n"
            + "  <File path=\"C:\\WexflowTesting\\Watchfolder1\\file1.txt\" name=\"file1.txt\" sha512=\"37f1ee35382e6e7918ce5cedbca431e58073af561b4681704df8ea495e048ddd0c890995302893beaecc946890673d1b6d6d2428fabe7ace0564c57a5454fb2f\" />\r\n"
            + "  <File path=\"C:\\WexflowTesting\\Watchfolder1\\file2.txt\" name=\"file2.txt\" sha512=\"37f1ee35382e6e7918ce5cedbca431e58073af561b4681704df8ea495e048ddd0c890995302893beaecc946890673d1b6d6d2428fabe7ace0564c57a5454fb2f\" />\r\n"
            + "  <File path=\"C:\\WexflowTesting\\Watchfolder1\\file3.mp4\" name=\"file3.mp4\" sha512=\"37f1ee35382e6e7918ce5cedbca431e58073af561b4681704df8ea495e048ddd0c890995302893beaecc946890673d1b6d6d2428fabe7ace0564c57a5454fb2f\" />\r\n"
            + "  <File path=\"C:\\WexflowTesting\\Watchfolder1\\file3.txt\" name=\"file3.txt\" sha512=\"37f1ee35382e6e7918ce5cedbca431e58073af561b4681704df8ea495e048ddd0c890995302893beaecc946890673d1b6d6d2428fabe7ace0564c57a5454fb2f\" />\r\n"
            + "</Files>";

        [TestInitialize]
        public void TestInitialize()
        {
            Helper.DeleteFiles(Sha512Folder);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helper.DeleteFiles(Sha512Folder);
        }

        [TestMethod]
        public void Sha512Test()
        {
            var files = GetFiles();
            Assert.AreEqual(0, files.Length);
            _ = Helper.StartWorkflow(48);
            files = GetFiles();
            Assert.AreEqual(1, files.Length);
            var content = File.ReadAllText(files[0]);
            Assert.AreEqual(ExpectedResult, content);
        }

        private static string[] GetFiles()
        {
            return Directory.GetFiles(Sha512Folder, "SHA512_*.xml");
        }
    }
}
