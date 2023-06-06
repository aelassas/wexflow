using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.Tests
{
    [TestClass]
    public class Untgz
    {
        private static readonly string TgzFolder = @"C:\WexflowTesting\Untgz_dest";

        [TestInitialize]
        public void TestInitialize()
        {
            Helper.DeleteFilesAndFolders(TgzFolder);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helper.DeleteFilesAndFolders(TgzFolder);
        }

        [TestMethod]
        public void UntgzTest()
        {
            var files = GetFiles();
            Assert.AreEqual(0, files.Length);
            _ = Helper.StartWorkflow(62);
            files = GetFiles();
            Assert.AreEqual(3, files.Length);
        }

        private string[] GetFiles()
        {
            return Directory.GetFiles(TgzFolder, "*.*", SearchOption.AllDirectories);
        }
    }
}
