using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.NetCore.Tests
{
    [TestClass]
    public class FilesMover
    {
        private static readonly string Src = @"C:\WexflowTesting\file10.txt";
        private static readonly string Dest = @"C:\WexflowTesting\FilesMover\file10.txt";

        [TestInitialize]
        public void TestInitialize()
        {
            if (File.Exists(Dest) && File.Exists(Src))
            {
                File.Delete(Dest);
            }
            else if (File.Exists(Dest) && !File.Exists(Src))
            {
                File.Move(Dest, Src);
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (File.Exists(Dest) && !File.Exists(Src))
            {
                File.Move(Dest, Src);
            }
        }

        [TestMethod]
        public void FilesMoverTest()
        {
            _ = Helper.StartWorkflow(4);
            Assert.AreEqual(true, File.Exists(Dest));
        }
    }
}
