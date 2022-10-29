using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.Tests
{
    [TestClass]
    public class Cron
    {
        private static readonly string CronFolder = @"C:\WexflowTesting\Cron";

        [TestInitialize]
        public void TestInitialize()
        {
            Helper.DeleteFiles(CronFolder);
            Helper.Run(); // Run Wexflow engine instance (cron)
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helper.Stop();
            Thread.Sleep(500);
            Helper.DeleteFiles(CronFolder);
        }

        [TestMethod]
        public void CronTest()
        {
            Thread.Sleep(90 * 1000); // 90 seconds
            var files = GetFiles(CronFolder);
            Assert.AreEqual(1, files.Length);
        }

        private string[] GetFiles(string folder)
        {
            return Directory.GetFiles(folder, "*.txt");
        }
    }
}
