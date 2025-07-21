using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.NetCore.Tests
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
        public async System.Threading.Tasks.Task ExecPythonTest()
        {
            // TODO
            _ = await Helper.StartWorkflow(175);
        }
    }
}
