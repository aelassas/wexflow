using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.Tests
{
    [TestClass]
    public class TextsEncryptor
    {
        private static readonly string TextsEncryptorFolder = @"C:\WexflowTesting\TextsEncryptor\";
        private static readonly string TextsDecryptorSrcFolder = @"C:\WexflowTesting\TextsDecryptor_src\";

        [TestInitialize]
        public void TestInitialize()
        {
            Helper.DeleteFiles(TextsEncryptorFolder);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helper.DeleteFiles(TextsEncryptorFolder);
            Helper.DeleteFiles(TextsDecryptorSrcFolder);
        }

        [TestMethod]
        public void TextsEncryptorTest()
        {
            var files = GetFiles(TextsEncryptorFolder);
            Assert.AreEqual(0, files.Length);
            _ = Helper.StartWorkflow(83);
            files = GetFiles(TextsEncryptorFolder);
            Assert.AreEqual(2, files.Length);
            files = GetFiles(TextsDecryptorSrcFolder);
            Assert.AreEqual(2, files.Length);
        }

        private string[] GetFiles(string dir)
        {
            return Directory.GetFiles(dir);
        }
    }
}
