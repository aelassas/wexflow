using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.NetCore.Tests
{
    [TestClass]
    public class RedditListComments
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
        public void RedditTest()
        {
            _ = Helper.StartWorkflow(130);
            // TODO
        }
    }
}
