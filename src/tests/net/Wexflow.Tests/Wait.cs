using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace Wexflow.Tests
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
            Stopwatch stopwatch = Stopwatch.StartNew();
            Helper.StartWorkflow(41);
            stopwatch.Stop();
            Assert.IsTrue(stopwatch.ElapsedMilliseconds > 30000);
        }
    }
}
