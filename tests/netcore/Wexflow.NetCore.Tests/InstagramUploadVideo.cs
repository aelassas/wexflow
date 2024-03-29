using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.NetCore.Tests
{
    [TestClass]
    public class InstagramUploadImage
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
            _ = Helper.StartWorkflow(123);
            // TODO
        }
    }
}
