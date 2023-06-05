using SharpScss;
using System;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.ScssToCss
{
    public class ScssToCss : Task
    {
        public ScssToCss(XElement xe, Workflow wf) : base(xe, wf)
        {
        }

        public override TaskStatus Run()
        {
            Info("Converting SCSS files to CSS files...");

            bool success;
            Status status = Status.Success;
            bool atLeastOneSuccess = false;
            try
            {
                success = ConvertFiles(ref atLeastOneSuccess);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while converting files.", e);
                success = false;
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

        private bool ConvertFiles(ref bool atLeastOneSuccess)
        {
            bool success = true;
            FileInf[] scssFiles = SelectFiles();

            foreach (FileInf scssFile in scssFiles)
            {
                try
                {
                    string source = File.ReadAllText(scssFile.Path);
                    ScssResult result = Scss.ConvertToCss(source);

                    string destPath = Path.Combine(Workflow.WorkflowTempFolder, Path.GetFileNameWithoutExtension(scssFile.FileName) + ".css");
                    File.WriteAllText(destPath, result.Css);
                    Files.Add(new FileInf(destPath, Id));
                    InfoFormat("The SCSS file {0} has been converted -> {1}", scssFile.Path, destPath);
                    if (!atLeastOneSuccess) atLeastOneSuccess = true;
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    ErrorFormat("An error occured while converting the SCSS file {0}: {1}", scssFile.Path, e.Message);
                    success = false;
                }
            }
            return success;
        }
    }
}
