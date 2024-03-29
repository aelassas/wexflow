using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.Tests
{
    [TestClass]
    public class Unrar
    {
        private static readonly string RarFolder = @"C:\WexflowTesting\Unrar_dest";

        [TestInitialize]
        public void TestInitialize()
        {
            Helper.DeleteFilesAndFolders(RarFolder);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helper.DeleteFilesAndFolders(RarFolder);
        }

        [TestMethod]
        public void UnrarTest()
        {
            var files = GetFiles();
            Assert.AreEqual(0, files.Length);
            _ = Helper.StartWorkflow(79);
            files = GetFiles();
            Assert.AreEqual(3, files.Length);
        }

        private string[] GetFiles()
        {
            return Directory.GetFiles(RarFolder, "*.*", SearchOption.AllDirectories);
        }
    }
}
