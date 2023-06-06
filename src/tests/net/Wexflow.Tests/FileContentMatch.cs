using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace Wexflow.Tests
{
    [TestClass]
    public class FileContentMatch
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
        public void FileContentMatchTest()
        {
            var stopwatch = Stopwatch.StartNew();
            _ = Helper.StartWorkflow(123);
            stopwatch.Stop();
            Assert.IsTrue(stopwatch.ElapsedMilliseconds > 1000);
            stopwatch.Reset();
            stopwatch.Start();
            _ = Helper.StartWorkflow(124);
            stopwatch.Stop();
            Assert.IsTrue(stopwatch.ElapsedMilliseconds > 2000);
        }
    }
}
