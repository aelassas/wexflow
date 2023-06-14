using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading;

namespace Wexflow.Tests
{
    [TestClass]
    public class Torrent
    {
        private static readonly string DownloadedFolder = @"C:\WexflowTesting\Torrent\The WIRED CD - Rip. Sample. Mash. Share";

        [TestInitialize]
        public void TestInitialize()
        {
            if (Directory.Exists(DownloadedFolder))
            {
                Helper.DeleteDirRec(DownloadedFolder);
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (Directory.Exists(DownloadedFolder))
            {
                Helper.DeleteDirRec(DownloadedFolder);
            }
        }

        [TestMethod]
        public void TorrentTest()
        {
            Assert.IsFalse(File.Exists(DownloadedFolder));
            _ = Helper.StartWorkflow(72);
            Assert.IsTrue(Directory.Exists(DownloadedFolder));
            Thread.Sleep(30 * 1000);
        }
    }
}
