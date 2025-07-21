using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.Tests
{
    [TestClass]
    public class HttpPatch
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
        public async System.Threading.Tasks.Task HttpPatchTest()
        {
            _ = await Helper.StartWorkflow(102);
            // TODO
        }
    }
}
