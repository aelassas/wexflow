using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.Tests
{
    [TestClass]
    public class ExecVb
    {
        private static readonly string DestDir = @"C:\WexflowTesting\ExecVb_dest\";
        private static readonly string File1 = @"C:\WexflowTesting\ExecVb_dest\file1.txt";
        private static readonly string File2 = @"C:\WexflowTesting\ExecVb_dest\file1.txt";
        private static readonly string Program1 = @"C:\WexflowTesting\ExecVb_dest\Program1.exe";
        private static readonly string Program2 = @"C:\WexflowTesting\ExecVb_dest\Program2.exe";

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
        public void ExecVbTest()
        {
            var files = GetFiles();
            Assert.AreEqual(0, files.Length);
            _ = Helper.StartWorkflow(99);
            files = GetFiles();
            Assert.AreEqual(4, files.Length);
            Assert.IsTrue(File.Exists(File1));
            Assert.IsTrue(File.Exists(File2));
            Assert.IsTrue(File.Exists(Program1));
            Assert.IsTrue(File.Exists(Program2));
        }

        private string[] GetFiles()
        {
            return Helper.GetFiles(DestDir, "*.*", SearchOption.TopDirectoryOnly);
        }
    }
}
