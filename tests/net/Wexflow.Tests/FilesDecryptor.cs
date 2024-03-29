using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.Tests
{
    [TestClass]
    public class FilesDecryptor
    {
        private static readonly string FilesDecryptorSrcFolder = @"C:\WexflowTesting\FilesDecryptor_src\";
        private static readonly string FilesDecryptorDestFolder = @"C:\WexflowTesting\FilesDecryptor_dest\";
        private static readonly string FilesEncryptorFolder = @"C:\WexflowTesting\FilesEncryptor\";

        [TestInitialize]
        public void TestInitialize()
        {
            Helper.DeleteFiles(FilesDecryptorSrcFolder);
            Helper.DeleteFiles(FilesDecryptorDestFolder);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helper.DeleteFiles(FilesDecryptorSrcFolder);
            Helper.DeleteFiles(FilesDecryptorDestFolder);
            Helper.DeleteFiles(FilesEncryptorFolder);
        }

        [TestMethod]
        public void FilesDecryptorTest()
        {
            var files = GetFiles(FilesDecryptorDestFolder);
            Assert.AreEqual(0, files.Length);
            _ = Helper.StartWorkflow(81);
            _ = Helper.StartWorkflow(82);
            files = GetFiles(FilesDecryptorDestFolder);
            Assert.AreEqual(3, files.Length);
        }

        private string[] GetFiles(string dir)
        {
            return Directory.GetFiles(dir);
        }
    }
}
