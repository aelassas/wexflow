using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.Tests
{
    [TestClass]
    public class UnSevenZip
    {
        private static readonly string SevenZipFolder = @"C:\WexflowTesting\UnSevenZip_dest";

        [TestInitialize]
        public void TestInitialize()
        {
            Helper.DeleteFilesAndFolders(SevenZipFolder);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helper.DeleteFilesAndFolders(SevenZipFolder);
        }

        [TestMethod]
        public void UnSevenZipTest()
        {
            var files = GetFiles();
            Assert.AreEqual(0, files.Length);
            _ = Helper.StartWorkflow(80);
            files = GetFiles();
            Assert.AreEqual(3, files.Length);
        }

        private string[] GetFiles()
        {
            return Directory.GetFiles(SevenZipFolder, "*.*", SearchOption.AllDirectories);
        }
    }
}
