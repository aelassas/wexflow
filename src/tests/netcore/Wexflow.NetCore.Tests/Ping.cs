using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace Wexflow.NetCore.Tests
{
    [TestClass]
    public class Ping
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
        public void PingTest()
        {
            var stopwatch = Stopwatch.StartNew();
            _ = Helper.StartWorkflow(94);
            stopwatch.Stop();
            Assert.IsTrue(stopwatch.ElapsedMilliseconds > 1000);
            stopwatch.Reset();
            stopwatch.Start();
            _ = Helper.StartWorkflow(95);
            stopwatch.Stop();
            Assert.IsTrue(stopwatch.ElapsedMilliseconds > 2000);
        }
    }
}
