using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace Wexflow.Tests
{
    [TestClass]
    public class Suspend
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
        public void SuspendTest()
        {
            var workflowId = 41;
            var instanceId = Helper.StartWorkflowAsync(workflowId);

            try
            {
                Thread.Sleep(500);
                var workflow = Helper.GetWorkflow(workflowId);
                Assert.IsFalse(workflow.IsPaused);
                Helper.SuspendWorkflow(workflowId, instanceId);
                Thread.Sleep(500);
                workflow = Helper.GetWorkflow(workflowId);
                Assert.IsTrue(workflow.IsPaused);
                Helper.ResumeWorkflow(workflowId, instanceId);
            }
            finally
            {
                Helper.StopWorkflow(workflowId, instanceId);
            }
        }
    }
}