using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.NetCore.Tests
{
    [TestClass]
    public class Untar
    {
        private static readonly string TarFolder = @"C:\WexflowTesting\Untar_dest";

        [TestInitialize]
        public void TestInitialize()
        {
            Helper.DeleteFilesAndFolders(TarFolder);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helper.DeleteFilesAndFolders(TarFolder);
        }

        [TestMethod]
        public void UntarTest()
        {
            var files = GetFiles();
            Assert.AreEqual(0, files.Length);
            _ = Helper.StartWorkflow(167);
            files = GetFiles();
            Assert.AreEqual(3, files.Length);
        }

        private static string[] GetFiles()
        {
            return Directory.GetFiles(TarFolder, "*.*", SearchOption.AllDirectories);
        }
    }
}
