using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.Tests
{
    [TestClass]
    public class MailsReceiver
    {
        private static readonly string MailsReceiverFolder = @"C:\WexflowTesting\MailsReceiver\";

        [TestInitialize]
        public void TestInitialize()
        {
            Helper.DeleteFiles(MailsReceiverFolder);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helper.DeleteFiles(MailsReceiverFolder);
        }

        [TestMethod]
        public void MailsReceiverTest()
        {
            string[] files = GetFiles();
            Assert.AreEqual(0, files.Length);
            Helper.StartWorkflow(56);
            files = GetFiles();
            Assert.IsTrue(files.Length > 0);
        }

        private string[] GetFiles()
        {
            return Directory.GetFiles(MailsReceiverFolder, "*.*");
        }
    }
}
