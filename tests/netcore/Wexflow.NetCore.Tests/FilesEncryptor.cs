﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.NetCore.Tests
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
        public async System.Threading.Tasks.Task FilesEncryptorTest()
        {
            var files = GetFiles(FilesEncryptorFolder);
            Assert.AreEqual(0, files.Length);
            _ = await Helper.StartWorkflow(81);
            files = GetFiles(FilesEncryptorFolder);
            Assert.AreEqual(3, files.Length);
            files = GetFiles(FilesDecryptorSrcFolder);
            Assert.AreEqual(3, files.Length);
        }

        private static string[] GetFiles(string dir)
        {
            return Directory.GetFiles(dir);
        }
    }
}
