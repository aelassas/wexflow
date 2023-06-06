using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.Tests
{
    [TestClass]
    public class YamlToJson
    {
        private static readonly string DestDir = @"C:\WexflowTesting\YamlToJson_dest\";
        private static readonly string File1 = @"C:\WexflowTesting\YamlToJson_dest\file1.json";
        private static readonly string File2 = @"C:\WexflowTesting\YamlToJson_dest\file2.json";

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
        public void YamlToJsonTest()
        {
            var files = GetFiles();
            Assert.AreEqual(0, files.Length);
            _ = Helper.StartWorkflow(110);
            files = GetFiles();
            Assert.AreEqual(2, files.Length);
            Assert.IsTrue(File.Exists(File1));
            Assert.IsTrue(File.Exists(File2));
        }

        private string[] GetFiles()
        {
            return Helper.GetFiles(DestDir, "*.*", SearchOption.TopDirectoryOnly);
        }
    }
}
