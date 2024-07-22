using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Globalization;

namespace Wexflow.NetCore.Tests
{
    [TestClass]
    public class Now
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
        public void NowTest()
        {
            var stopwatch = Stopwatch.StartNew();
            _ = Helper.StartWorkflow(51);
            stopwatch.Stop();

            var day = string.Format(new CultureInfo("en-US"), "{0:dddd}", DateTime.Now);
            switch (day)
            {
                case "Monday":
                    Assert.IsTrue(stopwatch.ElapsedMilliseconds is > 1000 and < 2000);
                    break;
                case "Tuesday":
                    Assert.IsTrue(stopwatch.ElapsedMilliseconds is > 2000 and < 3000);
                    break;
                case "Wednesday":
                    Assert.IsTrue(stopwatch.ElapsedMilliseconds is > 3000 and < 4000);
                    break;
                case "Thursday":
                    Assert.IsTrue(stopwatch.ElapsedMilliseconds is > 4000 and < 5000);
                    break;
                case "Friday":
                    Assert.IsTrue(stopwatch.ElapsedMilliseconds is > 5000 and < 6000);
                    break;
                case "Saturday":
                    Assert.IsTrue(stopwatch.ElapsedMilliseconds is > 6000 and < 7000);
                    break;
                case "Sunday":
                    Assert.IsTrue(stopwatch.ElapsedMilliseconds > 7000);
                    break;
                default:
                    break;
            }
        }
    }
}
