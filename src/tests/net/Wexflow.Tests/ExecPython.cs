using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.Tests
{
    [TestClass]
    public class ExecPython
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
        public void ExecCsTest()
        {
            _ = Helper.StartWorkflow(152);
            // TODO
        }
    }
}
