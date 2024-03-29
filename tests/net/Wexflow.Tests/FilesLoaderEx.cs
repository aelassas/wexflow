using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.Tests
{
    [TestClass]
    public class FilesLoaderEx
    {
        public static readonly string SourceFilesFolder =
            Path.Combine(Helper.SOURCE_FILES_FOLDER, "FilesLoaderEx") + Path.DirectorySeparatorChar;

        private static readonly string ExpectedResult138AddMaxCreateDate =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
            "<WexflowProcessing>\r\n" +
            "  <Workflow id=\"132\" name=\"Workflow_FilesLoaderEx_AddMaxCreateDate\" description=\"Workflow_FilesLoaderEx_AddMaxCreateDate\">\r\n" +
            "    <Files>\r\n" +
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file4.txt\" name=\"file4.txt\" renameTo=\"\" renameToOrName=\"file4.txt\" />\r\n" +
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file5.txt\" name=\"file5.txt\" renameTo=\"\" renameToOrName=\"file5.txt\" />\r\n" +
            "    </Files>\r\n" +
            "  </Workflow>\r\n" +
            "</WexflowProcessing>";

        private static readonly string ExpectedResult139AddMinCreateDate =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
            "<WexflowProcessing>\r\n" +
            "  <Workflow id=\"133\" name=\"Workflow_FilesLoaderEx_AddMinCreateDate\" description=\"Workflow_FilesLoaderEx_AddMinCreateDate\">\r\n" +
            "    <Files>\r\n" +
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file1.txt\" name=\"file1.txt\" renameTo=\"\" renameToOrName=\"file1.txt\" />\r\n" +
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file2.txt\" name=\"file2.txt\" renameTo=\"\" renameToOrName=\"file2.txt\" />\r\n" +
            "    </Files>\r\n" +
            "  </Workflow>\r\n" +
            "</WexflowProcessing>";

        private static readonly string ExpectedResult140AddMaxModifyDate =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
            "<WexflowProcessing>\r\n" +
            "  <Workflow id=\"134\" name=\"Workflow_FilesLoaderEx_AddMaxModifyDate\" description=\"Workflow_FilesLoaderEx_AddMaxModifyDate\">\r\n" +
            "    <Files>\r\n" +
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file4.txt\" name=\"file4.txt\" renameTo=\"\" renameToOrName=\"file4.txt\" />\r\n" +
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file5.txt\" name=\"file5.txt\" renameTo=\"\" renameToOrName=\"file5.txt\" />\r\n" +
            "    </Files>\r\n" +
            "  </Workflow>\r\n" +
            "</WexflowProcessing>";

        private static readonly string ExpectedResult141AddMinModifyDate =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
            "<WexflowProcessing>\r\n" +
            "  <Workflow id=\"135\" name=\"Workflow_FilesLoaderEx_AddMinModifyDate\" description=\"Workflow_FilesLoaderEx_AddMinModifyDate\">\r\n" +
            "    <Files>\r\n" +
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file1.txt\" name=\"file1.txt\" renameTo=\"\" renameToOrName=\"file1.txt\" />\r\n" +
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file2.txt\" name=\"file2.txt\" renameTo=\"\" renameToOrName=\"file2.txt\" />\r\n" +
            "    </Files>\r\n" +
            "  </Workflow>\r\n" +
            "</WexflowProcessing>";

        private static readonly string ExpectedResult142RemoveMaxCreateDate =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
            "<WexflowProcessing>\r\n" +
            "  <Workflow id=\"136\" name=\"Workflow_FilesLoaderEx_RemoveMaxCreateDate\" description=\"Workflow_FilesLoaderEx_RemoveMaxCreateDate\">\r\n" +
            "    <Files>\r\n" +
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file1.txt\" name=\"file1.txt\" renameTo=\"\" renameToOrName=\"file1.txt\" />\r\n" +
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file2.txt\" name=\"file2.txt\" renameTo=\"\" renameToOrName=\"file2.txt\" />\r\n" +
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file3.txt\" name=\"file3.txt\" renameTo=\"\" renameToOrName=\"file3.txt\" />\r\n" +
            "    </Files>\r\n" +
            "  </Workflow>\r\n" +
            "</WexflowProcessing>";

        private static readonly string ExpectedResult143RemoveMinCreateDate =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
            "<WexflowProcessing>\r\n" +
            "  <Workflow id=\"137\" name=\"Workflow_FilesLoaderEx_RemoveMinCreateDate\" description=\"Workflow_FilesLoaderEx_RemoveMinCreateDate\">\r\n" +
            "    <Files>\r\n" +
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file3.txt\" name=\"file3.txt\" renameTo=\"\" renameToOrName=\"file3.txt\" />\r\n" +
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file4.txt\" name=\"file4.txt\" renameTo=\"\" renameToOrName=\"file4.txt\" />\r\n" +
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file5.txt\" name=\"file5.txt\" renameTo=\"\" renameToOrName=\"file5.txt\" />\r\n" +
            "    </Files>\r\n" +
            "  </Workflow>\r\n" +
            "</WexflowProcessing>";

        private static readonly string ExpectedResult144RemoveMaxModifyDate =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
            "<WexflowProcessing>\r\n" +
            "  <Workflow id=\"138\" name=\"Workflow_FilesLoaderEx_RemoveMaxModifyDate\" description=\"Workflow_FilesLoaderEx_RemoveMaxModifyDate\">\r\n" +
            "    <Files>\r\n" +
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file1.txt\" name=\"file1.txt\" renameTo=\"\" renameToOrName=\"file1.txt\" />\r\n" +
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file2.txt\" name=\"file2.txt\" renameTo=\"\" renameToOrName=\"file2.txt\" />\r\n" +
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file3.txt\" name=\"file3.txt\" renameTo=\"\" renameToOrName=\"file3.txt\" />\r\n" +
            "    </Files>\r\n" +
            "  </Workflow>\r\n" +
            "</WexflowProcessing>";

        private static readonly string ExpectedResult145RemoveMinModifyDate =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
            "<WexflowProcessing>\r\n" +
            "  <Workflow id=\"139\" name=\"Workflow_FilesLoaderEx_RemoveMinModifyDate\" description=\"Workflow_FilesLoaderEx_RemoveMinModifyDate\">\r\n" +
            "    <Files>\r\n" +
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file3.txt\" name=\"file3.txt\" renameTo=\"\" renameToOrName=\"file3.txt\" />\r\n" +
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file4.txt\" name=\"file4.txt\" renameTo=\"\" renameToOrName=\"file4.txt\" />\r\n" +
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file5.txt\" name=\"file5.txt\" renameTo=\"\" renameToOrName=\"file5.txt\" />\r\n" +
            "    </Files>\r\n" +
            "  </Workflow>\r\n" +
            "</WexflowProcessing>";

        public void TestInitialize(int workflowId)
        {
            var tempFolder = Path.Combine(Helper.TEMP_FOLDER, workflowId.ToString());
            if (!Directory.Exists(tempFolder))
            {
                _ = Directory.CreateDirectory(tempFolder);
            }

            Helper.DeleteFilesAndFolders(tempFolder);
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        public void Execute(int workflowId, string expectedResult)
        {
            TestInitialize(workflowId);
            _ = Helper.StartWorkflow(workflowId);

            // Check the workflow result
            var files = Directory.GetFiles(
                Path.Combine(Helper.TEMP_FOLDER, workflowId.ToString()),
                "ListFiles*.xml",
                SearchOption.AllDirectories);
            Assert.AreEqual(1, files.Length);

            var content = File.ReadAllText(files[0]);
            Assert.AreEqual(expectedResult, content);
        }

        [TestMethod]
        public void FilesLoaderExTest_132AddMaxCreateDate()
        {
            Execute(132, ExpectedResult138AddMaxCreateDate);
        }

        [TestMethod]
        public void FilesLoaderExTest_133AddMinCreateDate()
        {
            Execute(133, ExpectedResult139AddMinCreateDate);
        }

        [TestMethod]
        public void FilesLoaderExTest_134AddMaxModifyDate()
        {
            Execute(134, ExpectedResult140AddMaxModifyDate);
        }

        [TestMethod]
        public void FilesLoaderExTest_135AddMinModifyDate()
        {
            Execute(135, ExpectedResult141AddMinModifyDate);
        }

        [TestMethod]
        public void FilesLoaderExTest_136RemoveMaxCreateDate()
        {
            Execute(136, ExpectedResult142RemoveMaxCreateDate);
        }

        [TestMethod]
        public void FilesLoaderExTest_137RemoveMinCreateDate()
        {
            Execute(137, ExpectedResult143RemoveMinCreateDate);
        }

        [TestMethod]
        public void FilesLoaderExTest_138RemoveMaxModifyDate()
        {
            Execute(138, ExpectedResult144RemoveMaxModifyDate);
        }

        [TestMethod]
        public void FilesLoaderExTest_139RemoveMinModifyDate()
        {
            Execute(139, ExpectedResult145RemoveMinModifyDate);
        }
    }
}
