using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.Tests
{
    [TestClass]
    public class FilesCopier
    {
        private string file1 = @"C:\WexflowTesting\FilesCopier\file1.txt";
        private string file2 = @"C:\WexflowTesting\FilesCopier\file2.txt";

        [TestInitialize]
        public void TestInitialize()
        {
            if (File.Exists(file1)) File.Delete(file1);
            if (File.Exists(file2)) File.Delete(file2);
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void FilesCopierTest()
        {
            Helper.StartWorkflow(2);

            // Check the workflow result
            Assert.AreEqual(true, File.Exists(file1));
            Assert.AreEqual(true, File.Exists(file2));
        }
    }
}
