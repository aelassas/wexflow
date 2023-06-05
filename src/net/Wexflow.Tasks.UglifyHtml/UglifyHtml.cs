using NUglify;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.UglifyHtml
{
    public class UglifyHtml : Task
    {
        public string SmbComputerName { get; private set; }
        public string SmbDomain { get; private set; }
        public string SmbUsername { get; private set; }
        public string SmbPassword { get; private set; }

        public UglifyHtml(XElement xe, Workflow wf) : base(xe, wf)
        {
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Uglifying HTML files...");


            bool success = true;
            bool atLeastOneSuccess = false;

            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        success = UglifyHtmlFiles(ref atLeastOneSuccess);
                    }
                }
                else
                {
                    success = UglifyHtmlFiles(ref atLeastOneSuccess);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while uglifying HTML files.", e);
                success = false;
            }

            Status status = Status.Success;
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

        private bool UglifyHtmlFiles(ref bool atLeastOneSuccess)
        {
            bool success = true;
            FileInf[] htmlFiles = SelectFiles();

            foreach (FileInf htmlFile in htmlFiles)
            {
                try
                {
                    string source = File.ReadAllText(htmlFile.Path);
                    UglifyResult result = Uglify.Html(source);
                    if (result.HasErrors)
                    {
                        ErrorFormat("An error occured while uglifying the HTML file {0}: {1}", htmlFile.Path, string.Concat(result.Errors.Select(e => e.Message + "\n").ToArray()));
                        success = false;
                        continue;
                    }

                    string destPath = Path.Combine(Workflow.WorkflowTempFolder, Path.GetFileNameWithoutExtension(htmlFile.FileName) + ".min.html");
                    File.WriteAllText(destPath, result.Code);
                    Files.Add(new FileInf(destPath, Id));
                    InfoFormat("The HTML file {0} has been uglified -> {1}", htmlFile.Path, destPath);
                    if (!atLeastOneSuccess) atLeastOneSuccess = true;
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    ErrorFormat("An error occured while uglifying the HTML file {0}: {1}", htmlFile.Path, e.Message);
                    success = false;
                }
            }
            return success;
        }
    }
}
