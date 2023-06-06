using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace Wexflow.NetCore.Tests
{
    [TestClass]
    public class MessageCorrect
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
        public void MessageCorrectTest()
        {
            var stopwatch = Stopwatch.StartNew();
            _ = Helper.StartWorkflow(117);
            stopwatch.Stop();
            Assert.IsTrue(stopwatch.ElapsedMilliseconds > 1000);
        }
    }
}
