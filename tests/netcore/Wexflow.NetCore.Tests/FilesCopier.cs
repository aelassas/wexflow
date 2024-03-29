using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.NetCore.Tests
{
    [TestClass]
    public class FilesCopier
    {
        private readonly string _file1 = @"C:\WexflowTesting\FilesCopier\file1.txt";
        private readonly string _file2 = @"C:\WexflowTesting\FilesCopier\file2.txt";

        [TestInitialize]
        public void TestInitialize()
        {
            if (File.Exists(_file1))
            {
                File.Delete(_file1);
            }

            if (File.Exists(_file2))
            {
                File.Delete(_file2);
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void FilesCopierTest()
        {
            _ = Helper.StartWorkflow(2);

            // Check the workflow result
            Assert.AreEqual(true, File.Exists(_file1));
            Assert.AreEqual(true, File.Exists(_file2));
        }
    }
}
