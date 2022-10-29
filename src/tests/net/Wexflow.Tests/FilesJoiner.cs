using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.Tests
{
    [TestClass]
    public class FilesJoiner
    {
        private static readonly string TempFolder = Path.Combine(Helper.TempFolder, "140");

        private static readonly string SourceFilesFolder =
            Path.Combine(Helper.SourceFilesFolder, "FilesJoiner") + Path.DirectorySeparatorChar;

        private const string ExpectedResultFileA = "file-a-1\r\n" +
                                                   "file-a-2\r\n" +
                                                   "file-a-3\r\n" +
                                                   "file-a-4\r\n" +
                                                   "file-a-5\r\n" +
                                                   "file-a-6\r\n" +
                                                   "file-a-7\r\n" +
                                                   "file-a-8\r\n" +
                                                   "file-a-9\r\n" +
                                                   "file-a-10\r\n" +
                                                   "file-a-11";

        private const string ExpectedResultFileB = "file-b-1\r\n" +
                                                   "file-b-2\r\n" +
                                                   "file-b-3";

        private const string ExpectedResultFileC = "file-c-1";
        private const string ExpectedResultFileD = "file-d";

        [TestInitialize]
        public void TestInitialize()
        {
            Helper.DeleteFilesAndFolders(TempFolder);
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void FilesJoinerTest()
        {
            var files = Directory.GetFiles(SourceFilesFolder);
            Assert.AreEqual(16, files.Length);

            Helper.StartWorkflow(140);

            files = Directory.GetFiles(TempFolder, "*", SearchOption.AllDirectories).OrderBy(f => f).ToArray();
            Assert.AreEqual(4, files.Length);

            var content = File.ReadAllText(files[0]);
            Assert.AreEqual(ExpectedResultFileA, content);

            content = File.ReadAllText(files[1]);
            Assert.AreEqual(ExpectedResultFileB, content);

            content = File.ReadAllText(files[2]);
            Assert.AreEqual(ExpectedResultFileC, content);

            content = File.ReadAllText(files[3]);
            Assert.AreEqual(ExpectedResultFileD, content);
        }
    }
}
