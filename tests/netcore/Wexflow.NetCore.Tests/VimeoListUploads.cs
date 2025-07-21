using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.NetCore.Tests
{
    [TestClass]
    public class VimeoListUploads
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
        public async System.Threading.Tasks.Task VimeoListUploadsTest()
        {
            // TODO
            _ = await Helper.StartWorkflow(135);
        }
    }
}
