using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace Wexflow.Tests
{
    [TestClass]
    public class FolderExists
    {
        [TestInitialize]
        public void TestInitialize()
        {
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void FolderExistsTest()
        {
            var stopwatch = Stopwatch.StartNew();
            _ = Helper.StartWorkflow(121);
            stopwatch.Stop();
            Assert.IsTrue(stopwatch.ElapsedMilliseconds > 1000);
            stopwatch.Reset();
            stopwatch.Start();
            _ = Helper.StartWorkflow(122);
            stopwatch.Stop();
            Assert.IsTrue(stopwatch.ElapsedMilliseconds > 2000);
        }
    }
}
