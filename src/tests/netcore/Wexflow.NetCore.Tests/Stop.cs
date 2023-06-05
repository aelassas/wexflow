using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.NetCore.Tests
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
            // Not Supported on .NET Core
            //int workflowId = 41;
            //try
            //{
            //    Helper.StartWorkflowAsync(workflowId);
            //    Thread.Sleep(500);
            //    var workflow = Helper.GetWorkflow(workflowId);
            //    Assert.IsTrue(workflow.IsRunning);
            //    Helper.StopWorkflow(workflowId);
            //    Thread.Sleep(500);
            //    workflow = Helper.GetWorkflow(workflowId);
            //    Assert.IsFalse(workflow.IsRunning);
            //}
            //finally
            //{
            //    Helper.StopWorkflow(workflowId);
            //}
        }
    }
}
