using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace Wexflow.NetCore.Tests
{
    [TestClass]
    public class EnvironmentVariable
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
        public void EnvironmentVariableTest()
        {
            var stopwatch = Stopwatch.StartNew();
            _ = Helper.StartWorkflow(116);
            stopwatch.Stop();

            var varValue = Environment.GetEnvironmentVariable("OS");
            switch (varValue)
            {
                case "Windows_NT":
                    Assert.IsTrue(stopwatch.ElapsedMilliseconds > 1000);
                    break;
                default:
                    Assert.IsTrue(stopwatch.ElapsedMilliseconds > 2000);
                    break;
            }
        }
    }
}
