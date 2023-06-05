using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.NetCore.Tests
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
            // Not Supported on .NET Core
            //int workflowId = 41;

            //try
            //{
            //    Helper.StartWorkflowAsync(workflowId);
            //    Thread.Sleep(500);
            //    var workflow = Helper.GetWorkflow(workflowId);
            //    Assert.IsFalse(workflow.IsPaused);
            //    Helper.SuspendWorkflow(workflowId);
            //    Thread.Sleep(500);
            //    workflow = Helper.GetWorkflow(workflowId);
            //    Assert.IsTrue(workflow.IsPaused);
            //    Helper.ResumeWorkflow(workflowId);
            //}
            //finally
            //{
            //    Helper.StopWorkflow(workflowId);
            //}
        }
    }
}