using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.Tests
{
    [TestClass]
    public class FilesEncryptor
    {
        private static readonly string FilesEncryptorFolder = @"C:\WexflowTesting\FilesEncryptor\";
        private static readonly string FilesDecryptorSrcFolder = @"C:\WexflowTesting\FilesDecryptor_src\";

        [TestInitialize]
        public void TestInitialize()
        {
            Helper.DeleteFiles(FilesEncryptorFolder);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helper.DeleteFiles(FilesEncryptorFolder);
            Helper.DeleteFiles(FilesDecryptorSrcFolder);
        }

        [TestMethod]
        public void FilesEncryptorTest()
        {
            var files = GetFiles(FilesEncryptorFolder);
            Assert.AreEqual(0, files.Length);
            _ = Helper.StartWorkflow(81);
            files = GetFiles(FilesEncryptorFolder);
            Assert.AreEqual(3, files.Length);
            files = GetFiles(FilesDecryptorSrcFolder);
            Assert.AreEqual(3, files.Length);
        }

        private string[] GetFiles(string dir)
        {
            return Directory.GetFiles(dir);
        }
    }
}
