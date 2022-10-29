using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.Tests
{
    [TestClass]
    public class DatabaseBackup
    {
        private static readonly string BackupFilePath = @"C:\WexflowTesting\DatabaseBackup\HELLOWORLD.bak";

        [TestInitialize]
        public void TestInitialize()
        {
            if (File.Exists(BackupFilePath)) File.Delete(BackupFilePath);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            File.Delete(BackupFilePath);
        }

        [TestMethod]
        public void DatabaseBackupTest()
        {
            Assert.IsFalse(File.Exists(BackupFilePath));
            Helper.StartWorkflow(85);
            Assert.IsTrue(File.Exists(BackupFilePath));
        }

    }
}
