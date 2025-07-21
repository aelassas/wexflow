using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.Tests
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
        public async System.Threading.Tasks.Task InstagramUploadImageTest()
        {
            _ = await Helper.StartWorkflow(120);
            // TODO
        }
    }
}
