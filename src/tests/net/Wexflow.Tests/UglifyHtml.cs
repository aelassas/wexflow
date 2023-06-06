using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.Tests
{
    [TestClass]
    public class UglifyHtml
    {
        private static readonly string DestDir = @"C:\WexflowTesting\UglifyHtml_dest\";
        private static readonly string File1 = @"C:\WexflowTesting\UglifyHtml_dest\wexflow-designer.min.html";
        private static readonly string File2 = @"C:\WexflowTesting\UglifyHtml_dest\wexflow-manager.min.html";

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
        public void UglifyHtmlTest()
        {
            var files = GetFiles();
            Assert.AreEqual(0, files.Length);
            _ = Helper.StartWorkflow(106);
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
