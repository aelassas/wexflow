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
            var workflowId = 41;
            var instanceId = Helper.StartWorkflowAsync(workflowId);

            try
            {
                Thread.Sleep(500);
                var workflow = Helper.GetWorkflow(workflowId);
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
