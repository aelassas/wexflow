using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.Tests
{
    [TestClass]
    public class Unzip
    {
        private static readonly string ZipFolder = @"C:\WexflowTesting\Unzip_dest";

        [TestInitialize]
        public void TestInitialize()
        {
            Helper.DeleteFilesAndFolders(ZipFolder);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helper.DeleteFilesAndFolders(ZipFolder);
        }

        [TestMethod]
        public void UnzipTest()
        {
            var files = GetFiles();
            Assert.AreEqual(0, files.Length);
            _ = Helper.StartWorkflow(60);
            files = GetFiles();
            Assert.AreEqual(3, files.Length);
        }

        private string[] GetFiles()
        {
            return Directory.GetFiles(ZipFolder, "*.*", SearchOption.AllDirectories);
        }
    }
}
