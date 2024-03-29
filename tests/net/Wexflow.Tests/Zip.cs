using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.Tests
{
    [TestClass]
    public class Zip
    {
        private static readonly string DestDir = @"C:\WexflowTesting\Zip";
        private static readonly string ZipFile = @"C:\WexflowTesting\Zip\output.zip";

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
        public void ZipTest()
        {
            Assert.AreEqual(false, File.Exists(ZipFile));
            _ = Helper.StartWorkflow(19);
            Assert.AreEqual(true, File.Exists(ZipFile));
        }
    }
}
