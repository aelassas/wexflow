using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.Tests
{
    [TestClass]
    public class Workflow
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
        public void WorkflowTest()
        {
            _ = Helper.StartWorkflow(45);
            // TODO
        }
    }
}
