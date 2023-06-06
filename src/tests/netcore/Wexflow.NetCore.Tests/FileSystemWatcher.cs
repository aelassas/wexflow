using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.NetCore.Tests
{
    [TestClass]
    public class FileSystemWatcher
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
        public void CsvToSqlTest()
        {
            // TODO
            _ = Helper.StartWorkflow(151);
        }

    }
}
