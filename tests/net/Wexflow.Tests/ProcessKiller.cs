using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace Wexflow.Tests
{
    [TestClass]
    public class ProcessKiller
    {
        [TestInitialize]
        public void TestInitialize()
        {
            Helper.StartProcess(@"C:\Windows\System32\notepad.exe", "", false);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helper.StartProcess("taskkill", "/im \"notepad.exe\" /f", true);
        }

        [TestMethod]
        public void ProcessKillerTest()
        {
            _ = Helper.StartWorkflow(58);
            var notepadProcesses = Process.GetProcessesByName("notepad");
            Assert.IsTrue(notepadProcesses.Length == 0);
        }
    }
}
