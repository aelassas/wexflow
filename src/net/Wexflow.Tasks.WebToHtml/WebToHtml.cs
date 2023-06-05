using OpenQA.Selenium.Chrome;
using System;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.WebToHtml
{
    public class WebToHtml : Task
    {
        public string[] Urls { get; private set; }

        public WebToHtml(XElement xe, Workflow wf) : base(xe, wf)
        {
            Urls = GetSettings("url");
        }

        public override TaskStatus Run()
        {
            Info("Getting HTML sources...");
            Status status = Status.Success;

            bool success = true;
            bool atLeastOneSuccess = false;

            foreach (string url in Urls)
            {
                try
                {
                    ChromeDriver driver = new ChromeDriver();
                    driver.Navigate().GoToUrl(url);

                    string destFile = Path.Combine(Workflow.WorkflowTempFolder,
                         string.Format("WebToHtml_{0:yyyy-MM-dd-HH-mm-ss-fff}.html", DateTime.Now));

                    string source = driver.PageSource;
                    File.WriteAllText(destFile, source);

                    if (!atLeastOneSuccess) atLeastOneSuccess = true;

                    InfoFormat("HTML source of {0} retrieved with success -> {1}", url, destFile);
                    Files.Add(new FileInf(destFile, Id));
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    ErrorFormat("An error occured while retrieving the source of {0}: {1}", url, e.Message);
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
