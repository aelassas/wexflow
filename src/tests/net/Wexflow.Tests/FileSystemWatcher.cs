using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.Tests
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
            _ = Helper.StartWorkflow(144);
        }

    }
}
