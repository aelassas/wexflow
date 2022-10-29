using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.Tests
{
    [TestClass]
    public class ScssToCss
    {
        private static readonly string DestDir = @"C:\WexflowTesting\ScssToCss_dest\";
        private static readonly string File1 = @"C:\WexflowTesting\ScssToCss_dest\file1.css";
        private static readonly string File2 = @"C:\WexflowTesting\ScssToCss_dest\file2.css";

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
        public void ScssToCssTest()
        {
            var files = GetFiles();
            Assert.AreEqual(0, files.Length);
            Helper.StartWorkflow(109);
            files = GetFiles();
            Assert.AreEqual(2, files.Length);
            Assert.IsTrue(File.Exists(File1));
            Assert.IsTrue(File.Exists(File2));
        }

        private string[] GetFiles()
        {
            return Helper.GetFiles(DestDir, "*.*", SearchOption.TopDirectoryOnly);
        }
    }
}
