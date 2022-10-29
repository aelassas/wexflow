using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace Wexflow.Tests
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
            Stopwatch stopwatch = Stopwatch.StartNew();
            Helper.StartWorkflow(116);
            stopwatch.Stop();

            string varValue = Environment.GetEnvironmentVariable("OS");
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
