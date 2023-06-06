using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.Tests
{
    [TestClass]
    public class Movedir
    {
        private static readonly string Src = @"C:\WexflowTesting\Folder";
        private static readonly string Dest = @"C:\WexflowTesting\Folder1";

        [TestInitialize]
        public void TestInitialize()
        {
            if (Directory.Exists(Dest))
            {
                Helper.DeleteFilesAndFolders(Dest);
                Directory.Delete(Dest);
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Directory.Move(Dest, Src);
        }

        [TestMethod]
        public void MovedirTest()
        {
            Assert.AreEqual(true, Directory.Exists(Src));
            Assert.AreEqual(false, Directory.Exists(Dest));
            _ = Helper.StartWorkflow(44);
            Assert.AreEqual(false, Directory.Exists(Src));
            Assert.AreEqual(true, Directory.Exists(Dest));
        }
    }
}
