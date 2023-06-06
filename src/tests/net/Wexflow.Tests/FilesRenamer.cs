using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.Tests
{
    [TestClass]
    public class FilesRenamer
    {
        private static readonly string Src = @"C:\WexflowTesting\file1.txt";
        private static readonly string SrcRenamed = @"C:\WexflowTesting\file1_renamed.txt";
        private static readonly string Tmp = @"C:\WexflowTesting\FilesRenamer\file1.txt";

        [TestInitialize]
        public void TestInitialize()
        {
            if (!File.Exists(Tmp))
            {
                File.Copy(Src, Tmp);
            }

            if (File.Exists(SrcRenamed))
            {
                File.Delete(SrcRenamed);
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            File.Copy(Tmp, Src);
            File.Delete(Tmp);
            File.Delete(SrcRenamed);
        }

        [TestMethod]
        public void FilesRenamerTest()
        {
            Assert.AreEqual(false, File.Exists(SrcRenamed));
            _ = Helper.StartWorkflow(36);
            Assert.AreEqual(true, File.Exists(SrcRenamed));
        }
    }
}