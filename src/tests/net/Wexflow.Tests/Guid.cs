using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Wexflow.Tests
{
    [TestClass]
    public class Guid
    {
        private static readonly string GuidFolder = @"C:\WexflowTesting\Guid\";

        [TestInitialize]
        public void TestInitialize()
        {
            Helper.DeleteFiles(GuidFolder);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Helper.DeleteFiles(GuidFolder);
        }

        [TestMethod]
        public void GuidTest()
        {
            var files = GetFiles();
            Assert.AreEqual(0, files.Length);
            _ = Helper.StartWorkflow(68);
            files = GetFiles();
            Assert.AreEqual(1, files.Length);
            var xdoc = XDocument.Load(files[0]);
            var xguids = xdoc.Descendants("Guid").ToList();
            Assert.AreEqual(3, xguids.Count);
            var regexPattern = "^([0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12})$";

            foreach (var xguid in xguids)
            {
                Assert.IsTrue(Regex.IsMatch(xguid.Value, regexPattern));
            }
        }

        private string[] GetFiles()
        {
            return Directory.GetFiles(GuidFolder, "Guid_*.xml");
        }
    }
}