using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.NetCore.Tests
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
            string[] files = GetFiles();
            Assert.AreEqual(0, files.Length);
            Helper.StartWorkflow(168);
            files = GetFiles();
            Assert.AreEqual(3, files.Length);
        }

        private string[] GetFiles()
        {
            return Directory.GetFiles(TgzFolder, "*.*", SearchOption.AllDirectories);
        }
    }
}
