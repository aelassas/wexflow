using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.Tests
{
    [TestClass]
    public class Vimeo
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
        public async System.Threading.Tasks.Task VimeoTest()
        {
            // TODO
            _ = await Helper.StartWorkflow(129);
        }
    }
}
