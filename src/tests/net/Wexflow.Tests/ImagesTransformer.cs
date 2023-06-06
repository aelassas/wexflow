using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.Tests
{
    [TestClass]
    public class ImagesTransformer
    {
        private static readonly string Dest1 = @"C:\WexflowTesting\ImagesTransformer\image1.png";
        private static readonly string Dest2 = @"C:\WexflowTesting\ImagesTransformer\image2.png";

        [TestInitialize]
        public void TestInitialize()
        {
            if (File.Exists(Dest1))
            {
                File.Delete(Dest1);
            }

            if (File.Exists(Dest2))
            {
                File.Delete(Dest2);
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (File.Exists(Dest1))
            {
                File.Delete(Dest1);
            }

            if (File.Exists(Dest2))
            {
                File.Delete(Dest2);
            }
        }

        [TestMethod]
        public void ImagesTransformerTest()
        {
            Assert.AreEqual(false, File.Exists(Dest1));
            Assert.AreEqual(false, File.Exists(Dest2));
            _ = Helper.StartWorkflow(24);
            Assert.AreEqual(true, File.Exists(Dest1));
            Assert.AreEqual(true, File.Exists(Dest2));
        }
    }
}
