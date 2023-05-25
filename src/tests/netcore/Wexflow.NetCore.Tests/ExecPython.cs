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
        public void CsvToJsonTest()
        {
            Helper.StartWorkflow(175);
        }
    }
}
