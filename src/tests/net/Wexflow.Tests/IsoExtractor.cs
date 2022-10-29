using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.Tests
{
    [TestClass]
    public class IsoExtractor
    {
        private static readonly string IsoFolder = @"C:\WexflowTesting\IsoExtractor_dest";

        [TestInitialize]
        public void TestInitialize()
        {
            Helper.DeleteFilesAndFolders(IsoFolder);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helper.DeleteFilesAndFolders(IsoFolder);
        }

        [TestMethod]
        public void IsoExtractorTest()
        {
            string[] files = GetFiles();
            Assert.AreEqual(0, files.Length);
            Helper.StartWorkflow(88);
            files = GetFiles();
            Assert.AreEqual(10, files.Length);
        }

        private string[] GetFiles()
        {
            return Directory.GetFiles(IsoFolder, "*.*", SearchOption.AllDirectories);
        }
    }
}
