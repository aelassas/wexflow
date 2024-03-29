using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace Wexflow.Tests
{
    [TestClass]
    public class Wmi
    {
        private static readonly string WmiFolder = @"C:\WexflowTesting\Wmi";

        private static string _expectedResult =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n"
            + "<Objects>\r\n"
            + "  <Object>\r\n"
            + "    <Property name=\"Name\" value=\"{0}\" />\r\n"
            + "  </Object>\r\n"
            + "</Objects>";

        [TestInitialize]
        public void TestInitialize()
        {
            Helper.DeleteFilesAndFolders(WmiFolder);
            _expectedResult = string.Format(_expectedResult, GetComputerName());
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helper.DeleteFilesAndFolders(WmiFolder);
        }

        [TestMethod]
        public void WmiTest()
        {
            var files = GetFiles();
            Assert.AreEqual(0, files.Length);
            _ = Helper.StartWorkflow(23);
            files = GetFiles();
            Assert.AreEqual(1, files.Length);
            var content = File.ReadAllText(files[0]);
            Assert.AreEqual(_expectedResult, content);
        }

        private string[] GetFiles()
        {
            return Directory.GetFiles(WmiFolder, "WMI_*.xml");
        }

        private string GetComputerName()
        {
            return Environment.MachineName;
        }
    }
}
