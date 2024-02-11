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
        public string Dir { get; }
        public string Pattern { get; }
        public bool Recursive { get; }

        public FileMatch(XElement xe, Workflow wf) : base(xe, wf)
        {
            Dir = GetSetting("dir");
            Pattern = GetSetting("pattern");
            Recursive = bool.Parse(GetSetting("recursive", "false"));
        }

        public override TaskStatus Run()
        {
            Info("Checking file...");

            var success = false;
            var fileFound = string.Empty;

            try
            {
                var files = Recursive
                    ? Directory.GetFiles(Dir, "*.*", SearchOption.AllDirectories)
                    : Directory.GetFiles(Dir, "*.*", SearchOption.TopDirectoryOnly);
                foreach (var file in files)
                {
                    if (Regex.Match(Path.GetFileName(file), Pattern).Success)
                    {
                        fileFound = file;
                        success = true;
                        break;
                    }

                    WaitOne();
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
            catch (ThreadInterruptedException)
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
