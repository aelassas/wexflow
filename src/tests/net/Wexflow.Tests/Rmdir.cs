using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            if (!Directory.Exists(Temp)) Directory.CreateDirectory(Temp);
            Helper.CopyDirRec(Src, Temp);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helper.CopyDirRec(Path.Combine(Temp, "Rmdir") , @"C:\WexflowTesting");
        }

        [TestMethod]
        public void RmdirTest()
        {
            Assert.AreEqual(true, Directory.Exists(Src));
            Helper.StartWorkflow(14);
            Assert.AreEqual(false, Directory.Exists(Src));
        }
    }
}
