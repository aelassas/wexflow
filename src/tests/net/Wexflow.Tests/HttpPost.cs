using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.Tests
{
    [TestClass]
    public class HttpPost
    {
        //private static readonly string DestDir = @"C:\WexflowTesting\FilesCopier\";
        //private static readonly string File1 = @"C:\WexflowTesting\FilesCopier\file1.txt";
        //private static readonly string File2 = @"C:\WexflowTesting\FilesCopier\file2.txt";

        [TestInitialize]
        public void TestInitialize()
        {
            //Helper.DeleteFiles(DestDir);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            //Helper.DeleteFiles(DestDir);
        }

        //
        // Must start Wexflow server for this test???
        //

        [TestMethod]
        public void HttpPostTest()
        {
            // TODO
            //var files = GetFiles();
            //Assert.AreEqual(0, files.Length);
            _ = Helper.StartWorkflow(100);
            //files = GetFiles();
            //Assert.AreEqual(2, files.Length);
            //Assert.IsTrue(File.Exists(File1));
            //Assert.IsTrue(File.Exists(File2));
        }

        //private string[] GetFiles()
        //{
        //    return Helper.GetFiles(DestDir, "*.*", SearchOption.TopDirectoryOnly);
        //}
    }
}
