using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.NetCore.Tests
{
    [TestClass]
    public class ListEntities
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
        public void ListEntitiesTest()
        {
            _ = Helper.StartWorkflow(7);
            // TODO
        }
    }
}
