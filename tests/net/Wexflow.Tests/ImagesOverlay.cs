using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using System.IO;

namespace Wexflow.Tests
{
    [TestClass]
    public class ImagesOverlay
    {
        private static readonly string DestFolder = @"C:\WexflowTesting\ImagesOverlayDest\";

        [TestInitialize]
        public void TestInitialize()
        {
            Helper.DeleteFiles(DestFolder);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helper.DeleteFiles(DestFolder);
        }

        [TestMethod]
        public void ImagesOverlayTest()
        {
            var images = GetFiles();
            Assert.AreEqual(0, images.Length);
            _ = Helper.StartWorkflow(78);
            images = GetFiles();
            Assert.AreEqual(1, images.Length);

            // Checking the image size
            using (var image = Image.FromFile(images[0]))
            {
                Assert.AreEqual(1024, image.Width);
                Assert.AreEqual(768, image.Height);
            }
        }

        private string[] GetFiles()
        {
            return Directory.GetFiles(DestFolder, "*.png");
        }

        //private void CheckImageSize(string path)
        //{
        //    using (Image image = Image.FromFile(path))
        //    {
        //        Assert.AreEqual(512, image.Width);
        //        Assert.AreEqual(384, image.Height);
        //    }
        //}
    }
}
