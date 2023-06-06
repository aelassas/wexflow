using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.Tests
{
    [TestClass]
    public class Twilio
    {
        [TestInitialize]
        public void TestInitialize()
        {
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void TwilioTest()
        {
            _ = Helper.StartWorkflow(141);
            // TODO
        }
    }
}
