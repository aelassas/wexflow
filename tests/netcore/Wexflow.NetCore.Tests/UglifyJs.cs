﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.NetCore.Tests
{
    [TestClass]
    public class UglifyJs
    {
        private static readonly string DestDir = @"C:\WexflowTesting\UglifyJs_dest\";
        private static readonly string File1 = @"C:\WexflowTesting\UglifyJs_dest\wexflow-designer.min.js";
        private static readonly string File2 = @"C:\WexflowTesting\UglifyJs_dest\wexflow-manager.min.js";

        [TestInitialize]
        public void TestInitialize()
        {
            Helper.DeleteFiles(DestDir);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helper.DeleteFiles(DestDir);
        }

        [TestMethod]
        public async System.Threading.Tasks.Task UglifyJsTest()
        {
            var files = GetFiles();
            Assert.AreEqual(0, files.Length);
            _ = await Helper.StartWorkflow(166);
            files = GetFiles();
            Assert.AreEqual(2, files.Length);
            Assert.IsTrue(File.Exists(File1));
            Assert.IsTrue(File.Exists(File2));
        }

        private static string[] GetFiles()
        {
            return Helper.GetFiles(DestDir, "*.*", SearchOption.TopDirectoryOnly);
        }
    }
}
