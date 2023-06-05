using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Threading;

namespace Wexflow.NetCore.Tests
{
    [TestClass]
    public class Approval
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
        public void ApprovalTest()
        {
            int workflowId = 131;
            System.Guid instanceId = Helper.StartWorkflow(workflowId);
            Thread.Sleep(500);
            Helper.ApproveWorkflow(workflowId, instanceId);
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
            Assert.IsTrue(stopwatch.ElapsedMilliseconds > 2000);
        }
    }
}
