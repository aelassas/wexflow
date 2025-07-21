using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.Tests
{
    [TestClass]
    public class IsoCreator
    {
        private static readonly string IsoPath = @"C:\WexflowTesting\IsoCreator_dest\Wexflow_sample.iso";

        [TestInitialize]
        public void TestInitialize()
        {
            if (File.Exists(IsoPath))
            {
                File.Delete(IsoPath);
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            File.Delete(IsoPath);
        }

        [TestMethod]
        public async System.Threading.Tasks.Task IsoCreatorTest()
        {
            Assert.IsFalse(File.Exists(IsoPath));
            _ = await Helper.StartWorkflow(87);
            Assert.IsTrue(File.Exists(IsoPath));
        }
    }
}
