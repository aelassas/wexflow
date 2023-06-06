using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.Tests
{
    [TestClass]
    public class ProcessLauncher
    {
        private static readonly string Mp3Folder = @"C:\WexflowTesting\MP3";
        private static readonly string Dest = @"C:\WexflowTesting\MP3\kof.mp3";

        [TestInitialize]
        public void TestInitialize()
        {
            Helper.DeleteFiles(Mp3Folder);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helper.DeleteFiles(Mp3Folder);
        }

        [TestMethod]
        public void ProcessLauncherTest()
        {
            var files = GetFiles();
            Assert.AreEqual(0, files.Length);
            _ = Helper.StartWorkflow(12);
            files = GetFiles();
            Assert.AreEqual(1, files.Length);
            Assert.AreEqual(true, File.Exists(Dest));
        }

        private string[] GetFiles()
        {
            return Directory.GetFiles(Mp3Folder, "*.mp3");
        }
    }
}
