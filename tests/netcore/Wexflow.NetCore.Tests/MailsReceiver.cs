using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.NetCore.Tests
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
        [Ignore]
        public async System.Threading.Tasks.Task MailsReceiverTest()
        {
            var files = GetFiles();
            Assert.AreEqual(0, files.Length);
            _ = await Helper.StartWorkflow(56);
            files = GetFiles();
            Assert.IsTrue(files.Length > 0);
        }

        private static string[] GetFiles()
        {
            return Directory.GetFiles(MailsReceiverFolder, "*.*");
        }
    }
}
