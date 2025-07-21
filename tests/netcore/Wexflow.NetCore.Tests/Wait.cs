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
        public async System.Threading.Tasks.Task WaitTest()
        {
            var stopwatch = Stopwatch.StartNew();
            _ = await Helper.StartWorkflow(41);
            stopwatch.Stop();
            Assert.IsTrue(stopwatch.ElapsedMilliseconds > 30000);
        }
    }
}
