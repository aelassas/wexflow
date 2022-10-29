using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.Tests
{
    [TestClass]
    public class Tar
    {
        private static readonly string DestDir = @"C:\WexflowTesting\Tar";
        private static readonly string TarFile = @"C:\WexflowTesting\Tar\output.tar";

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
        public void TarTest()
        {
            Assert.AreEqual(false, File.Exists(TarFile));
            Helper.StartWorkflow(20);
            Assert.AreEqual(true, File.Exists(TarFile));
        }
    }
}
