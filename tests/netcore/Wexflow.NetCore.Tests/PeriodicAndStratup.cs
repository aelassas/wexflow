using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading;

namespace Wexflow.NetCore.Tests
{
    [TestClass]
    public class PeriodicAndStratup
    {
        private static readonly string StartupFolder = @"C:\WexflowTesting\Startup";
        private static readonly string PeriodicFolder = @"C:\WexflowTesting\Periodic";

        private static string GetPeriodicWorkflowXml(bool enabled)
        {
            return $@"<Workflow xmlns='urn:wexflow-schema' id='59' name='Workflow_Periodic' description='Workflow_Periodic'>
	            <Settings>
		            <Setting name='launchType' value='periodic' />
		            <Setting name='period' value='00.00:00:05' />
		            <Setting name='enabled' value='{enabled.ToString().ToLower()}' />
	            </Settings>
	            <Tasks>
		            <Task id='1' name='FilesLoader' description='Loading files' enabled='true'>
		            <Setting name='file' value='C:\WexflowTesting\file1.txt' />
		            </Task>
		            <Task id='2' name='FilesCopier' description='Copying files' enabled='true'>
		            <Setting name='selectFiles' value='1' />
		            <Setting name='destFolder' value='C:\WexflowTesting\Periodic' />
		            <Setting name='overwrite' value='true' />
		            </Task>
	            </Tasks>
            </Workflow>";
        }

        [TestInitialize]
        public async System.Threading.Tasks.Task TestInitialize()
        {
            Helper.DeleteFiles(StartupFolder);
            Helper.DeleteFilesAndFolders(PeriodicFolder);

            await Helper.Run(); // Run Wexflow engine instance (startup+periodic)

            var wf = GetPeriodicWorkflowXml(true);
            await Helper.SaveWorkflow(wf, true);
        }

        [TestCleanup]
        public async System.Threading.Tasks.Task TestCleanup()
        {
            var wf = GetPeriodicWorkflowXml(false);
            await Helper.SaveWorkflow(wf, true);

            await Helper.Stop();
            Thread.Sleep(500);
            Helper.DeleteFiles(StartupFolder);
            Helper.DeleteFilesAndFolders(PeriodicFolder);
        }

        [TestMethod]
        public void PeriodicAndStratupTest()
        {
            // Periodic test
            Thread.Sleep(7 * 1000);
            var files = GetFiles(PeriodicFolder);
            Assert.AreEqual(1, files.Length);

            // Startup test
            Thread.Sleep(1000);
            files = GetFiles(StartupFolder);
            Assert.AreEqual(1, files.Length);
        }

        private static string[] GetFiles(string folder)
        {
            return Directory.GetFiles(folder, "*.txt");
        }
    }
}
