using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.Tests
{
    [TestClass]
    public class PeriodicAndStratup
    {
        private static readonly string StartupFolder = @"C:\WexflowTesting\Startup";
        private static readonly string PeriodicFolder = @"C:\WexflowTesting\Periodic";

        [TestInitialize]
        public void TestInitialize()
        {
            Helper.DeleteFiles(StartupFolder);
            Helper.DeleteFilesAndFolders(PeriodicFolder);
            Helper.Run(); // Run Wexflow engine instance (startup+periodic)
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helper.Stop();
            Thread.Sleep(500);
            Helper.DeleteFiles(StartupFolder);
            Helper.DeleteFilesAndFolders(PeriodicFolder);
        }

        [TestMethod]
        public void PeriodicAndStratupTest()
        {
            // Startup test
            Thread.Sleep(1000);
            var files = GetFiles(StartupFolder);
            Assert.AreEqual(1, files.Length);

            // Periodic test
            Thread.Sleep(2000);
            files = GetFiles(PeriodicFolder);
            Assert.AreEqual(1, files.Length);
        }

        private string[] GetFiles(string folder)
        {
            return Directory.GetFiles(folder, "*.txt");
        }
    }
}
