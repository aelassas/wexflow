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
            int workflowId = 126;
            System.Guid instanceId = Helper.StartWorkflow(workflowId);
            Thread.Sleep(500);
            Helper.RejectWorkflow(workflowId, instanceId);
            Stopwatch stopwatch = Stopwatch.StartNew();
            Core.Workflow workflow = Helper.GetWorkflow(workflowId);
            bool isRunning = workflow.IsRunning;
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
