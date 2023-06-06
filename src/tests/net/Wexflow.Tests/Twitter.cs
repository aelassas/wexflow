using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.Tests
{
    [TestClass]
    public class Twitter
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
        public void TwitterTest()
        {
            _ = Helper.StartWorkflow(16);
            // TODO
        }
    }
}
