using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.NetCore.Tests
{
    [TestClass]
    public class CsvToYaml
    {
        private static readonly string DestDir = @"C:\WexflowTesting\CsvToYaml_dest\";
        private static readonly string File1 = @"C:\WexflowTesting\CsvToYaml_dest\file1.yml";
        private static readonly string File2 = @"C:\WexflowTesting\CsvToYaml_dest\file2.yml";

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
        public void CsvToYamlTest()
        {
            var files = GetFiles();
            Assert.AreEqual(0, files.Length);
            _ = Helper.StartWorkflow(154);
            files = GetFiles();
            Assert.AreEqual(2, files.Length);
            Assert.IsTrue(File.Exists(File1));
            Assert.IsTrue(File.Exists(File2));
        }

        private static string[] GetFiles()
        {
            return Helper.GetFiles(DestDir, "*.*", SearchOption.TopDirectoryOnly);
        }
    }
}
