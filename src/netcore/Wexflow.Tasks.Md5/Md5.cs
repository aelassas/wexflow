using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.Md5
{
    public class Md5 : Task
    {
        public Md5(XElement xe, Workflow wf)
            : base(xe, wf)
        {
        }

        public override TaskStatus Run()
        {
            Info("Generating MD5 sums...");

            var success = true;
            var atLeastOneSucceed = false;

            var files = SelectFiles();

            if (files.Length > 0)
            {
                var md5Path = Path.Combine(Workflow.WorkflowTempFolder,
                    $"MD5_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}.xml");

                XDocument xdoc = new(new XElement("Files"));
                foreach (var file in files)
                {
                    try
                    {
                        var md5 = GetMd5(file.Path);
                        xdoc.Root?.Add(new XElement("File",
                                new XAttribute("path", file.Path),
                                new XAttribute("name", file.FileName),
                                new XAttribute("md5", md5)));
                        InfoFormat("Md5 of the file {0} is {1}", file.Path, md5);

                        if (!atLeastOneSucceed)
                        {
                            atLeastOneSucceed = true;
                        }
                    }
                    catch (ThreadInterruptedException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        ErrorFormat("An error occured while generating the md5 of the file {0}", e, file.Path);
                        success = false;
                    }
                    finally
                    {
                        WaitOne();
                    }
                }
                xdoc.Save(md5Path);
                Files.Add(new FileInf(md5Path, Id));
            }

            var status = Status.Success;

            if (!success && atLeastOneSucceed)
            {
                status = Status.Warning;
            }
            else if (!success)
            {
                status = Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status, false);
        }

        private static string GetMd5(string filePath)
        {
            StringBuilder sb = new();
#pragma warning disable CA5351 // Ne pas utiliser d'algorithmes de chiffrement cassés
            using (var md5 = MD5.Create())
            {
                using var stream = File.OpenRead(filePath);
                var bytes = md5.ComputeHash(stream);

                foreach (var bt in bytes)
                {
                    _ = sb.Append(bt.ToString("x2"));
                }
            }
#pragma warning restore CA5351 // Ne pas utiliser d'algorithmes de chiffrement cassés
            return sb.ToString();
        }
    }
}
