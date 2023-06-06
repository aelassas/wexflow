using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.Tests
{
    [TestClass]
    public class Touch
    {
        private static readonly string File1 = @"C:\WexflowTesting\Triggers\trigger1.empty";
        private static readonly string File2 = @"C:\WexflowTesting\Triggers\trigger2.empty";

        [TestInitialize]
        public void TestInitialize()
        {
            if (File.Exists(File1))
            {
                File.Delete(File1);
            }

            if (File.Exists(File2))
            {
                File.Delete(File2);
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            File.Delete(File1);
            File.Delete(File2);
        }

        [TestMethod]
        public void TouchTest()
        {
            Assert.AreEqual(false, File.Exists(File1));
            Assert.AreEqual(false, File.Exists(File2));
            _ = Helper.StartWorkflow(15);
            Assert.AreEqual(true, File.Exists(File1));
            Assert.AreEqual(true, File.Exists(File2));
        }
    }
}
