using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.NetCore.Tests
{
    [TestClass]
    public class InstagramUploadVideo
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
        public void InstagramUploadImageTest()
        {
            _ = Helper.StartWorkflow(122);
            // TODO
        }
    }
}
