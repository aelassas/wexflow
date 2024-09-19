using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;
using System.IO;
using System.Runtime.Versioning;

namespace Wexflow.NetCore.Tests
{
    [TestClass]
    [SupportedOSPlatform("windows")]
    public class ImagesConcat
    {
        private static readonly string DestFolder = @"C:\WexflowTesting\ImagesConcatDest\";

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
        public void ImagesConcatTest()
        {
            var images = GetFiles();
            Assert.AreEqual(0, images.Length);
            _ = Helper.StartWorkflow(77);
            images = GetFiles();
            Assert.AreEqual(1, images.Length);

            // Checking the image size
            using var image = SKImage.FromEncodedData(images[0]);
            Assert.AreEqual(3072, image.Width);
            Assert.AreEqual(768, image.Height);
        }

        private static string[] GetFiles()
        {
            return Directory.GetFiles(DestFolder, "*.jpg");
        }

        //private static void CheckImageSize(string path)
        //{
        //    using var image = Image.FromFile(path);
        //    Assert.AreEqual(512, image.Width);
        //    Assert.AreEqual(384, image.Height);
        //}
    }
}
