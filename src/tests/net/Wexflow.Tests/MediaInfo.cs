using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.Tests
{
    [TestClass]
    public class MediaInfo
    {
        private static readonly string MediaInfoFolder = @"C:\WexflowTesting\MediaInfo\";

        [TestInitialize]
        public void TestInitialize()
        {
            Helper.DeleteFiles(MediaInfoFolder);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helper.DeleteFiles(MediaInfoFolder);
        }

        [TestMethod]
        public void MediaInfoTest()
        {
            //string[] files = GetFiles();
            //Assert.AreEqual(0, files.Length);
            //Helper.StartWorkflow(52);
            //files = GetFiles();
            //Assert.AreEqual(1, files.Length);
            //string content = File.ReadAllText(files[0]);

            //string expectedResultPath = Path.Combine(MediaInfoFolder, System.Guid.NewGuid() + ".xml");
            //var xdoc = Tasks.MediaInfo.MediaInfo.Inform(new[]
            //{
            //    new FileInf(@"C:\WexflowTesting\WAV\kof.wav", 1),
            //    new FileInf(@"C:\WexflowTesting\MP4\small.mp4", 1)
            //});
            //xdoc.Save(expectedResultPath);
            //var expectedResult = File.ReadAllText(expectedResultPath);

            //Assert.AreEqual(expectedResult, content);

            // TODO Tasks.MediaInfo.MediaInfo.Inform
        }

        //private string[] GetFiles()
        //{
        //    return Directory.GetFiles(MediaInfoFolder, "MediaInfo_*.xml");
        //}
    }
}
