using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.FileMatch
{
    public class FileMatch : Task
    {
        public string Dir { get; private set; }
        public string Pattern { get; private set; }
        public bool Recursive { get; private set; }

        public FileMatch(XElement xe, Workflow wf) : base(xe, wf)
        {
            Dir = GetSetting("dir");
            Pattern = GetSetting("pattern");
            Recursive = bool.Parse(GetSetting("recursive", "false"));
        }

        public override TaskStatus Run()
        {
            Info("Checking file...");

            bool success = false;
            string fileFound = string.Empty;

            try
            {
                string[] files;

                if (Recursive)
                {
                    files = Directory.GetFiles(Dir, "*.*", SearchOption.AllDirectories);
                }
                else
                {
                    files = Directory.GetFiles(Dir, "*.*", SearchOption.TopDirectoryOnly);
                }

                foreach (var file in files)
                {
                    if (Regex.Match(Path.GetFileName(file), Pattern).Success)
                    {
                        fileFound = file;
                        success = true;
                        break;
                    }
                }


                if (success)
                {
                    InfoFormat("A file matching the pattern {0} was found in the directory {1} -> {2}.", Pattern, Dir, fileFound);
                }
                else
                {
                    InfoFormat("No file was found in the directory {0} matching the pattern {1}.", Dir, Pattern);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while checking directory {0}. Error: {1}", Dir, e.Message);
                return new TaskStatus(Status.Error, false);
            }

            Info("Task finished");

            return new TaskStatus(Status.Success, success);
        }
    }
}
