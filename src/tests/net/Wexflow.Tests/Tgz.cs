using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.Tests
{
    [TestClass]
    public class Tgz
    {
        private static readonly string DestDir = @"C:\WexflowTesting\Tgz";
        private static readonly string TgzFile = @"C:\WexflowTesting\Tgz\output.tar.gz";

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
        public void TgzTest()
        {
            Assert.AreEqual(false, File.Exists(TgzFile));
            Helper.StartWorkflow(21);
            Assert.AreEqual(true, File.Exists(TgzFile));
        }
    }
}
