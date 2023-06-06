using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.Tests
{
    [TestClass]
    public class HttpGet
    {
        private static readonly string DestDir = @"C:\WexflowTesting\HttpGet\";

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
        public void HttpGetTest()
        {
            var files = GetFiles();
            Assert.AreEqual(0, files.Length);
            _ = Helper.StartWorkflow(108);
            files = GetFiles();
            Assert.AreEqual(1, files.Length);
        }

        private string[] GetFiles()
        {
            return Helper.GetFiles(DestDir, "*.*", SearchOption.TopDirectoryOnly);
        }
    }
}
