using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.NetCore.Tests
{
    [TestClass]
    public class MailsSender
    {
        private static readonly string MailsSenderFolder = @"C:\WexflowTesting\MailsSender\";

        [TestInitialize]
        public void TestInitialize()
        {
            Helper.DeleteFiles(MailsSenderFolder);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helper.DeleteFiles(MailsSenderFolder);
        }

        [TestMethod]
        [Ignore]
        public void MailsSenderTest()
        {
            var files = GetFiles();
            Assert.AreEqual(0, files.Length);
            _ = Helper.StartWorkflow(9);
            files = GetFiles();
            Assert.AreEqual(2, files.Length);
        }

        private static string[] GetFiles()
        {
            return Directory.GetFiles(MailsSenderFolder, "*.txt");
        }
    }
}
