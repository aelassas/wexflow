using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.Sha1
{
    public class Sha1 : Task
    {
        public Sha1(XElement xe, Workflow wf)
            : base(xe, wf)
        {
        }

        public override TaskStatus Run()
        {
            Info("Generating SHA-1 hashes...");

            var success = true;
            var atLeastOneSucceed = false;

            var files = SelectFiles();

            if (files.Length > 0)
            {
                var md5Path = Path.Combine(Workflow.WorkflowTempFolder,
                    $"SHA1_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}.xml");

                XDocument xdoc = new(new XElement("Files"));
                foreach (var file in files)
                {
                    try
                    {
                        var sha1 = GetSha1(file.Path);
                        xdoc.Root?.Add(new XElement("File",
                                new XAttribute("path", file.Path),
                                new XAttribute("name", file.FileName),
                                new XAttribute("sha1", sha1)));
                        InfoFormat("SHA-1 hash of the file {0} is {1}", file.Path, sha1);

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
                        ErrorFormat("An error occured while generating the SHA-1 hash of the file {0}", e, file.Path);
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

        private static string GetSha1(string filePath)
        {
            using var stream = File.OpenRead(filePath);
#pragma warning disable CA5350 // Ne pas utiliser d'algorithmes de chiffrement faibles
            using var alg = SHA1.Create();
#pragma warning restore CA5350 // Ne pas utiliser d'algorithmes de chiffrement faibles
            StringBuilder sb = new();

            var hashValue = alg.ComputeHash(stream);
            foreach (var bt in hashValue)
            {
                _ = sb.Append(bt.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
