using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace Wexflow.Tests
{
    [TestClass]
    public class FilesJoiner
    {
        private static readonly string TempFolder = Path.Combine(Helper.TEMP_FOLDER, "140");

        private static readonly string SourceFilesFolder =
            Path.Combine(Helper.SOURCE_FILES_FOLDER, "FilesJoiner") + Path.DirectorySeparatorChar;

        private const string EXPECTED_RESULT_FILE_A = "file-a-1\r\n" +
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

        private const string EXPECTED_RESULT_FILE_B = "file-b-1\r\n" +
                                                   "file-b-2\r\n" +
                                                   "file-b-3";

        private const string EXPECTED_RESULT_FILE_C = "file-c-1";
        private const string EXPECTED_RESULT_FILE_D = "file-d";

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

            _ = Helper.StartWorkflow(140);

            files = Directory.GetFiles(TempFolder, "*", SearchOption.AllDirectories).OrderBy(f => f).ToArray();
            Assert.AreEqual(4, files.Length);

            var content = File.ReadAllText(files[0]);
            Assert.AreEqual(EXPECTED_RESULT_FILE_A, content);

            content = File.ReadAllText(files[1]);
            Assert.AreEqual(EXPECTED_RESULT_FILE_B, content);

            content = File.ReadAllText(files[2]);
            Assert.AreEqual(EXPECTED_RESULT_FILE_C, content);

            content = File.ReadAllText(files[3]);
            Assert.AreEqual(EXPECTED_RESULT_FILE_D, content);
        }
    }
}
