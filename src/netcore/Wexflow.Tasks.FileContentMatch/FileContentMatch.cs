using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.FileContentMatch
{
    public class FileContentMatch : Task
    {
        public string[] FilesToCheck { get; }
        public string[] FoldersToCheck { get; }
        public bool Recursive { get; }
        public string Pattern { get; }

        public FileContentMatch(XElement xe, Workflow wf) : base(xe, wf)
        {
            FilesToCheck = GetSettings("file");
            FoldersToCheck = GetSettings("folder");
            Recursive = bool.Parse(GetSetting("recursive", "false"));
            Pattern = GetSetting("pattern");
        }

        public override TaskStatus Run()
        {
            Info("Checking file...");

            var success = true;
            try
            {
                // Checking files
                foreach (var file in FilesToCheck)
                {
                    var res = Regex.Match(File.ReadAllText(file), Pattern, RegexOptions.Multiline).Success;

                    if (res)
                    {
                        InfoFormat("A content matching the pattern {0} was found in the file {1}.", Pattern, file);
                        Files.Add(new FileInf(file, Id));
                    }
                    else
                    {
                        InfoFormat("No content matching the pattern {0} was found in the file {1}.", Pattern, file);
                    }

                    WaitOne();
                }

                // Checking folders
                foreach (var folder in FoldersToCheck)
                {
                    var files = Recursive ? Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories) : Directory.GetFiles(folder);

                    foreach (var file in files)
                    {
                        var res = Regex.Match(File.ReadAllText(file), Pattern, RegexOptions.Multiline).Success;

                        if (res)
                        {
                            InfoFormat("A content matching the pattern {0} was found in the file {1}.", Pattern, file);
                            Files.Add(new FileInf(file, Id));
                        }
                        else
                        {
                            InfoFormat("No content matching the pattern {0} was found in the file {1}.", Pattern, file);
                        }

                        WaitOne();
                    }
                }

                if (Files.Count == 0)
                {
                    success = false;
                }
            }
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while checking thes files. Error: {0}", e.Message);
                return new TaskStatus(Status.Error, false);
            }

            Info("Task finished");

            return new TaskStatus(Status.Success, success);
        }
    }
}
