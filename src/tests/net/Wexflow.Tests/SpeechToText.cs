using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.Tests
{
    [TestClass]
    public class SpeechToText
    {
        private static readonly string DestDir = @"C:\WexflowTesting\SpeechToText_dest";
        private static readonly string TextFile = @"C:\WexflowTesting\SpeechToText_dest\file.txt";

        [TestInitialize]
        public void TestInitialize()
        {
            Helper.DeleteFiles(DestDir);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helper.DeleteFiles(DestDir);
        }

        [TestMethod]
        public void SpeechToTextTest()
        {
            Assert.AreEqual(false, File.Exists(TextFile));
            _ = Helper.StartWorkflow(91);
            Assert.AreEqual(true, File.Exists(TextFile));
        }
    }
}
