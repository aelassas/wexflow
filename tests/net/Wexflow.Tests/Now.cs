using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Globalization;

namespace Wexflow.Tests
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
                    Assert.IsTrue(stopwatch.ElapsedMilliseconds > 1000 && stopwatch.ElapsedMilliseconds < 2000);
                    break;
                case "Tuesday":
                    Assert.IsTrue(stopwatch.ElapsedMilliseconds > 2000 && stopwatch.ElapsedMilliseconds < 3000);
                    break;
                case "Wednesday":
                    Assert.IsTrue(stopwatch.ElapsedMilliseconds > 3000 && stopwatch.ElapsedMilliseconds < 4000);
                    break;
                case "Thursday":
                    Assert.IsTrue(stopwatch.ElapsedMilliseconds > 4000 && stopwatch.ElapsedMilliseconds < 5000);
                    break;
                case "Friday":
                    Assert.IsTrue(stopwatch.ElapsedMilliseconds > 5000 && stopwatch.ElapsedMilliseconds < 6000);
                    break;
                case "Saturday":
                    Assert.IsTrue(stopwatch.ElapsedMilliseconds > 6000 && stopwatch.ElapsedMilliseconds < 7000);
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
