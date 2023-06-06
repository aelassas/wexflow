using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.Tests
{
    [TestClass]
    public class Mkdir
    {
        private static readonly string Folder1 = @"C:\WexflowTesting\Watchfolder4";
        private static readonly string Folder2 = @"C:\WexflowTesting\Watchfolder5";

        [TestInitialize]
        public void TestInitialize()
        {
            if (Directory.Exists(Folder1))
            {
                Directory.Delete(Folder1);
            }

            if (Directory.Exists(Folder2))
            {
                Directory.Delete(Folder2);
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Directory.Delete(Folder1);
            Directory.Delete(Folder2);
        }

        [TestMethod]
        public void MkdirTest()
        {
            Assert.AreEqual(false, Directory.Exists(Folder1));
            Assert.AreEqual(false, Directory.Exists(Folder2));
            _ = Helper.StartWorkflow(11);
            Assert.AreEqual(true, Directory.Exists(Folder1));
            Assert.AreEqual(true, Directory.Exists(Folder2));
        }
    }
}
