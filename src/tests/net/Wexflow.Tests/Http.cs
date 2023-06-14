using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.Tests
{
    [TestClass]
    public class Http
    {
        private static readonly string Dest = @"C:\WexflowTesting\Http\wexflow-cover.png";

        [TestInitialize]
        public void TestInitialize()
        {
            if (File.Exists(Dest))
            {
                File.Delete(Dest);
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            File.Delete(Dest);
        }

        [TestMethod]
        public void HttpTest()
        {
            Assert.AreEqual(false, File.Exists(Dest));
            _ = Helper.StartWorkflow(25);
            Assert.AreEqual(true, File.Exists(Dest));
        }
    }
}
