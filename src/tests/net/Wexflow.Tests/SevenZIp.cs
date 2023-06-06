using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.Tests
{
    [TestClass]
    public class SevenZip
    {
        private static readonly string DestDir = @"C:\WexflowTesting\SevenZip_dest";
        private static readonly string SevenZipFile = @"C:\WexflowTesting\SevenZip_dest\output.7z";

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
        public void SevenZipTest()
        {
            Assert.AreEqual(false, File.Exists(SevenZipFile));
            _ = Helper.StartWorkflow(89);
            Assert.AreEqual(true, File.Exists(SevenZipFile));
        }
    }
}
