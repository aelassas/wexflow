using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.WebToScreenshot
{
    public class WebToScreenshot : Task
    {
        public string[] Urls { get; private set; }

        public WebToScreenshot(XElement xe, Workflow wf) : base(xe, wf)
        {
            Urls = GetSettings("url");
        }

        public override TaskStatus Run()
        {
            Info("Taking screenshots...");
            var status = Status.Success;

            bool success = true;
            bool atLeastOneSuccess = false;

            foreach (var url in Urls)
            {
                try
                {
                    var driver = new ChromeDriver();

                    driver.Navigate().GoToUrl(url);
                    Screenshot ss = ((ITakesScreenshot)driver).GetScreenshot();

                    var destFile = Path.Combine(Workflow.WorkflowTempFolder,
                         string.Format("WebToScreenshot_{0:yyyy-MM-dd-HH-mm-ss-fff}.png", DateTime.Now));

                    ss.SaveAsFile(destFile, ScreenshotImageFormat.Png);

                    if (!atLeastOneSuccess) atLeastOneSuccess = true;

                    InfoFormat("Screenshot of {0} taken with success -> {1}", url, destFile);
                    Files.Add(new FileInf(destFile, Id));
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    ErrorFormat("An error occured while taking the screenshot of {0}: {1}", url, e.Message);
                    success = false;
                }
            }


            if (!success && atLeastOneSuccess)
            {
                status = Status.Warning;
            }
            else if (!success)
            {
                status = Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status);
        }
    }
}
