using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace Wexflow.Tests
{
    [TestClass]
    public class Stop
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
        public void StopTest()
        {
            int workflowId = 41;
            System.Guid instanceId = Helper.StartWorkflowAsync(workflowId);

            try
            {
                Thread.Sleep(500);
                Core.Workflow workflow = Helper.GetWorkflow(workflowId);
                Assert.IsTrue(workflow.IsRunning);
                Helper.StopWorkflow(workflowId, instanceId);
                Thread.Sleep(500);
                workflow = Helper.GetWorkflow(workflowId);
                Assert.IsFalse(workflow.IsRunning);
            }
            finally
            {
                Helper.StopWorkflow(workflowId, instanceId);
            }
        }
    }
}
