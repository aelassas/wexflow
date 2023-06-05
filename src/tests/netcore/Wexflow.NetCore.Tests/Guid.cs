using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Wexflow.NetCore.Tests
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
            string[] files = GetFiles();
            Assert.AreEqual(0, files.Length);
            Helper.StartWorkflow(68);
            files = GetFiles();
            Assert.AreEqual(1, files.Length);
            XDocument xdoc = XDocument.Load(files[0]);
            System.Collections.Generic.List<XElement> xguids = xdoc.Descendants("Guid").ToList();
            Assert.AreEqual(3, xguids.Count);
            string regexPattern = @"^([0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12})$";

            foreach (XElement xguid in xguids)
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