using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.NetCore.Tests
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
            _ = Helper.StartWorkflow(161);
            Assert.AreEqual(true, File.Exists(TarFile));
        }
    }
}
