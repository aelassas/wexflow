using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.NetCore.Tests
{
    [TestClass]
    public class FilesRemover
    {
        private static readonly string Src = @"C:\WexflowTesting\file11.txt";
        private static readonly string Tmp = @"C:\WexflowTesting\FilesRemover\file11.txt";

        [TestInitialize]
        public void TestInitialize()
        {
            if (!File.Exists(Tmp))
            {
                File.Copy(Src, Tmp);
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            File.Copy(Tmp, Src);
            if (File.Exists(Tmp))
            {
                File.Delete(Tmp);
            }
        }

        [TestMethod]
        public void FilesRemoverTest()
        {
            Assert.AreEqual(true, File.Exists(Src));
            _ = Helper.StartWorkflow(5);
            Assert.AreEqual(false, File.Exists(Src));
        }
    }
}
