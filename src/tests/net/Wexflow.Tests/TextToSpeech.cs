using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.Tests
{
    [TestClass]
    public class TextToSpeech
    {
        private static readonly string DestDir = @"C:\WexflowTesting\TextToSpeech_dest";
        private static readonly string WavFile = @"C:\WexflowTesting\TextToSpeech_dest\file.wav";

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
        public void TextToSpeechTest()
        {
            Assert.AreEqual(false, File.Exists(WavFile));
            _ = Helper.StartWorkflow(90);
            Assert.AreEqual(true, File.Exists(WavFile));
        }
    }
}
