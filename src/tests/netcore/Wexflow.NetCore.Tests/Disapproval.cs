using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.NetCore.Tests
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
            var workflowId = 132;
            var instanceId = Helper.StartWorkflow(workflowId);
            Thread.Sleep(500);
            Helper.RejectWorkflow(workflowId, instanceId);
            Stopwatch stopwatch = Stopwatch.StartNew();
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
