using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.NetCore.Tests
{
    [TestClass]
    public class FilesLoaderEx
    {
        public static readonly string SourceFilesFolder =
            Path.Combine(Helper.ColumnSourceFilesFolder, "FilesLoaderEx") + Path.DirectorySeparatorChar;

        private static readonly string ExpectedResult138AddMaxCreateDate =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
            "<WexflowProcessing>\r\n" +
            "  <Workflow id=\"139\" name=\"Workflow_FilesLoaderEx_AddMaxCreateDate\" description=\"Workflow_FilesLoaderEx_AddMaxCreateDate\">\r\n" +
            "    <Files>\r\n" +
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file4.txt\" name=\"file4.txt\" renameTo=\"\" renameToOrName=\"file4.txt\" />\r\n" +
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file5.txt\" name=\"file5.txt\" renameTo=\"\" renameToOrName=\"file5.txt\" />\r\n" +
            "    </Files>\r\n" +
            "  </Workflow>\r\n" +
            "</WexflowProcessing>";

        private static readonly string ExpectedResult139AddMinCreateDate =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
            "<WexflowProcessing>\r\n" +
            "  <Workflow id=\"140\" name=\"Workflow_FilesLoaderEx_AddMinCreateDate\" description=\"Workflow_FilesLoaderEx_AddMinCreateDate\">\r\n" +
            "    <Files>\r\n" +
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file1.txt\" name=\"file1.txt\" renameTo=\"\" renameToOrName=\"file1.txt\" />\r\n" +
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file2.txt\" name=\"file2.txt\" renameTo=\"\" renameToOrName=\"file2.txt\" />\r\n" +
            "    </Files>\r\n" +
            "  </Workflow>\r\n" +
            "</WexflowProcessing>";

        private static readonly string ExpectedResult140AddMaxModifyDate =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
            "<WexflowProcessing>\r\n" +
            "  <Workflow id=\"141\" name=\"Workflow_FilesLoaderEx_AddMaxModifyDate\" description=\"Workflow_FilesLoaderEx_AddMaxModifyDate\">\r\n" +
            "    <Files>\r\n" +
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file4.txt\" name=\"file4.txt\" renameTo=\"\" renameToOrName=\"file4.txt\" />\r\n" +
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file5.txt\" name=\"file5.txt\" renameTo=\"\" renameToOrName=\"file5.txt\" />\r\n" +
            "    </Files>\r\n" +
            "  </Workflow>\r\n" +
            "</WexflowProcessing>";

        private static readonly string ExpectedResult141AddMinModifyDate =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
            "<WexflowProcessing>\r\n" +
            "  <Workflow id=\"142\" name=\"Workflow_FilesLoaderEx_AddMinModifyDate\" description=\"Workflow_FilesLoaderEx_AddMinModifyDate\">\r\n" +
            "    <Files>\r\n" +
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file1.txt\" name=\"file1.txt\" renameTo=\"\" renameToOrName=\"file1.txt\" />\r\n" +
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file2.txt\" name=\"file2.txt\" renameTo=\"\" renameToOrName=\"file2.txt\" />\r\n" +
            "    </Files>\r\n" +
            "  </Workflow>\r\n" +
            "</WexflowProcessing>";

        private static readonly string ExpectedResult142RemoveMaxCreateDate =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
            "<WexflowProcessing>\r\n" +
            "  <Workflow id=\"143\" name=\"Workflow_FilesLoaderEx_RemoveMaxCreateDate\" description=\"Workflow_FilesLoaderEx_RemoveMaxCreateDate\">\r\n" +
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
            "  <Workflow id=\"144\" name=\"Workflow_FilesLoaderEx_RemoveMinCreateDate\" description=\"Workflow_FilesLoaderEx_RemoveMinCreateDate\">\r\n" +
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
            "  <Workflow id=\"145\" name=\"Workflow_FilesLoaderEx_RemoveMaxModifyDate\" description=\"Workflow_FilesLoaderEx_RemoveMaxModifyDate\">\r\n" +
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
            "  <Workflow id=\"146\" name=\"Workflow_FilesLoaderEx_RemoveMinModifyDate\" description=\"Workflow_FilesLoaderEx_RemoveMinModifyDate\">\r\n" +
            "    <Files>\r\n" +
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file3.txt\" name=\"file3.txt\" renameTo=\"\" renameToOrName=\"file3.txt\" />\r\n" +
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file4.txt\" name=\"file4.txt\" renameTo=\"\" renameToOrName=\"file4.txt\" />\r\n" +
            "      <File taskId=\"1\" path=\"" + SourceFilesFolder + "file5.txt\" name=\"file5.txt\" renameTo=\"\" renameToOrName=\"file5.txt\" />\r\n" +
            "    </Files>\r\n" +
            "  </Workflow>\r\n" +
            "</WexflowProcessing>";

        public static void TestInitialize(int workflowId)
        {
            var tempFolder = Path.Combine(Helper.ColumnTempFolder, workflowId.ToString());
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

        public static void Execute(int workflowId, string expectedResult)
        {
            TestInitialize(workflowId);
            _ = Helper.StartWorkflow(workflowId);

            // Check the workflow result
            var files = Directory.GetFiles(
                Path.Combine(Helper.ColumnTempFolder, workflowId.ToString()),
                "ListFiles*.xml",
                SearchOption.AllDirectories);
            Assert.AreEqual(1, files.Length);

            var content = File.ReadAllText(files[0]);
            Assert.AreEqual(expectedResult, content);
        }

        [TestMethod]
        public void FilesLoaderExTest_139AddMaxCreateDate()
        {
            Execute(139, ExpectedResult138AddMaxCreateDate);
        }

        [TestMethod]
        public void FilesLoaderExTest_140AddMinCreateDate()
        {
            Execute(140, ExpectedResult139AddMinCreateDate);
        }

        [TestMethod]
        public void FilesLoaderExTest_141AddMaxModifyDate()
        {
            Execute(141, ExpectedResult140AddMaxModifyDate);
        }

        [TestMethod]
        public void FilesLoaderExTest_142AddMinModifyDate()
        {
            Execute(142, ExpectedResult141AddMinModifyDate);
        }

        [TestMethod]
        public void FilesLoaderExTest_143RemoveMaxCreateDate()
        {
            Execute(143, ExpectedResult142RemoveMaxCreateDate);
        }

        [TestMethod]
        public void FilesLoaderExTest_144RemoveMinCreateDate()
        {
            Execute(144, ExpectedResult143RemoveMinCreateDate);
        }

        [TestMethod]
        public void FilesLoaderExTest_145RemoveMaxModifyDate()
        {
            Execute(145, ExpectedResult144RemoveMaxModifyDate);
        }

        [TestMethod]
        public void FilesLoaderExTest_146RemoveMinModifyDate()
        {
            Execute(146, ExpectedResult145RemoveMinModifyDate);
        }
    }
}
