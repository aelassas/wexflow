using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wexflow.Tests
{
    [TestClass]
    public class Rmdir
    {
        private static readonly string Src = @"C:\WexflowTesting\Rmdir";
        private static readonly string Temp = @"C:\Wexflow\Temp\14";

        [TestInitialize]
        public void TestInitialize()
        {
            if (!Directory.Exists(Temp))
            {
                _ = Directory.CreateDirectory(Temp);
            }

            Helper.CopyDirRec(Src, Temp);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helper.CopyDirRec(Path.Combine(Temp, "Rmdir"), @"C:\WexflowTesting");
        }

        [TestMethod]
        public void RmdirTest()
        {
            Assert.AreEqual(true, Directory.Exists(Src));
            _ = Helper.StartWorkflow(14);
            Assert.AreEqual(false, Directory.Exists(Src));
        }
    }
}
