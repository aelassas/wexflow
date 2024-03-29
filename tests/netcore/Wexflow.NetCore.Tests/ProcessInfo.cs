using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Wexflow.NetCore.Tests
{
    [TestClass]
    public class ProcessInfo
    {
        private static readonly string ProcessInfoFolder = @"C:\WexflowTesting\ProcessInfo\";

        [TestInitialize]
        public void TestInitialize()
        {
            Helper.DeleteFiles(ProcessInfoFolder);
            Helper.StartProcess(@"C:\Windows\System32\notepad.exe", "", false);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helper.DeleteFiles(ProcessInfoFolder);
            Helper.StartProcess("taskkill", "/im \"notepad.exe\" /f", true);
        }

        [TestMethod]
        public void ProcessInfoTest()
        {
            var files = GetFiles();
            Assert.AreEqual(0, files.Length);
            _ = Helper.StartWorkflow(63);
            files = GetFiles();
            Assert.AreEqual(1, files.Length);
            var xdoc = XDocument.Load(files[0]);
            var count = xdoc.Descendants("Process").Count();
            Assert.AreEqual(1, count);
        }

        private static string[] GetFiles()
        {
            return Directory.GetFiles(ProcessInfoFolder, "ProcessInfo_*.xml");
        }
    }
}
