using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading;

namespace Wexflow.NetCore.Tests
{
    [TestClass]
    public class Cron
    {
        private static readonly string CronFolder = @"C:\WexflowTesting\Cron";

        private static string GetCronWorkflowXml(bool enabled)
        {
            return $@"<?xml version='1.0' encoding='utf-8'?>
                    <Workflow xmlns='urn:wexflow-schema' id='75' name='Workflow_Cron' description='Workflow_Cron'>
                        <Settings>
                        <Setting name='launchType' value='cron' />
	                    <Setting name='cronExpression' value='0 0/1 * * * ?' /> <!-- Every one minute. -->
                        <Setting name='enabled' value='{enabled.ToString().ToLower()}' />
                        </Settings>
                        <Tasks>
                        <Task id='1' name='FilesLoader' description='Loading files' enabled='true'>
                            <Setting name='file' value='C:\WexflowTesting\file1.txt' />
                        </Task>
	                    <Task id='2' name='Wait' description='Wait for 10 seconds...' enabled='true'>
			                    <Setting name='duration' value='00.00:00:10' />
		                </Task>
                        <Task id='3' name='FilesCopier' description='Copying files' enabled='true'>
                            <Setting name='selectFiles' value='1' />
                            <Setting name='destFolder' value='C:\WexflowTesting\Cron' />
                            <Setting name='overwrite' value='true' />
                        </Task>
                        </Tasks>
                    </Workflow>";
        }

        [TestInitialize]
        public void TestInitialize()
        {
            Helper.DeleteFiles(CronFolder);
            Helper.Run(); // Run Wexflow engine instance (cron)

            var wf = GetCronWorkflowXml(true);
            Helper.SaveWorkflow(wf, true);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            var wf = GetCronWorkflowXml(false);
            Helper.SaveWorkflow(wf, true);

            Helper.Stop();
            Thread.Sleep(500);
            Helper.DeleteFiles(CronFolder);
        }

        [TestMethod]
        public void CronTest()
        {
            Thread.Sleep(70 * 1000); // 70 seconds
            var files = GetFiles(CronFolder);
            Assert.AreEqual(1, files.Length);
        }

        private static string[] GetFiles(string folder)
        {
            return Directory.GetFiles(folder, "*.txt");
        }
    }
}
