using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace Wexflow.Tests
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
        public async System.Threading.Tasks.Task PingTest()
        {
            var stopwatch = Stopwatch.StartNew();
            _ = await Helper.StartWorkflow(94);
            stopwatch.Stop();
            Assert.IsTrue(stopwatch.ElapsedMilliseconds > 1000);
            stopwatch.Reset();
            stopwatch.Start();
            _ = await Helper.StartWorkflow(95);
            stopwatch.Stop();
            Assert.IsTrue(stopwatch.ElapsedMilliseconds > 2000);
        }
    }
}
