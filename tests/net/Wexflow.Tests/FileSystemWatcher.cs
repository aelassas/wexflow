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
#pragma warning disable CS1998
        public async System.Threading.Tasks.Task FileSystemWatcherTest()
#pragma warning restore CS1998
        {
            // TODO
            //_ = await Helper.StartWorkflow(144);
        }
    }
}
