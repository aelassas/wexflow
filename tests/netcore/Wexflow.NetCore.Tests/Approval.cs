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
        public async System.Threading.Tasks.Task ApprovalTest()
        {
            var workflowId = 131;
            var instanceId = await Helper.StartWorkflow(workflowId);
            await System.Threading.Tasks.Task.Delay(500);
            Helper.ApproveWorkflow(workflowId, instanceId);
            var stopwatch = Stopwatch.StartNew();
            var workflow = Helper.GetWorkflow(workflowId);
            var isRunning = workflow.IsRunning;
            while (isRunning)
            {
                await System.Threading.Tasks.Task.Delay(100);
                workflow = Helper.GetWorkflow(workflowId);
                isRunning = workflow.IsRunning;
            }
            stopwatch.Stop();
            Assert.IsTrue(stopwatch.ElapsedMilliseconds > 2000);
        }
    }
}
