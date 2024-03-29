using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace Wexflow.NetCore.Tests
{
    [TestClass]
    public class Wait
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
        public void WaitTest()
        {
            var stopwatch = Stopwatch.StartNew();
            _ = Helper.StartWorkflow(41);
            stopwatch.Stop();
            Assert.IsTrue(stopwatch.ElapsedMilliseconds > 30000);
        }
    }
}
