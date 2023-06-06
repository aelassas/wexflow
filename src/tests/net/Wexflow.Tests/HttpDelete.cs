using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.Tests
{
    [TestClass]
    public class HttpDelete
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
        public void HttpDeleteTest()
        {
            _ = Helper.StartWorkflow(103);
            // TODO
        }
    }
}
