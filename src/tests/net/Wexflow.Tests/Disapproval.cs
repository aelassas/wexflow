using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Threading;

namespace Wexflow.Tests
{
    [TestClass]
    public class Disapproval
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
        public void DisapprovalTest()
        {
            var workflowId = 126;
            var instanceId = Helper.StartWorkflow(workflowId);
            Thread.Sleep(500);
            Helper.RejectWorkflow(workflowId, instanceId);
            var stopwatch = Stopwatch.StartNew();
            var workflow = Helper.GetWorkflow(workflowId);
            var isRunning = workflow.IsRunning;
            while (isRunning)
            {
                Thread.Sleep(100);
                workflow = Helper.GetWorkflow(workflowId);
                isRunning = workflow.IsRunning;
            }
            stopwatch.Stop();
            Assert.IsTrue(stopwatch.ElapsedMilliseconds > 3000);
        }
    }
}
