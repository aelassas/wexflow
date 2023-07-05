using System;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.FilesInfo
{
    public class FilesInfo : Task
    {
        public string SmbComputerName { get; }
        public string SmbDomain { get; }
        public string SmbUsername { get; }
        public string SmbPassword { get; }

        public FilesInfo(XElement xe, Workflow wf) : base(xe, wf)
        {
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Generating files informations...");

            bool success;
            var atLeastOneSucceed = false;

            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        success = GenerateInfo(ref atLeastOneSucceed);
                    }
                }
                else
                {
                    success = GenerateInfo(ref atLeastOneSucceed);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while generating files info.", e);
                success = false;
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

        private bool GenerateInfo(ref bool atLeastOneSucceed)
        {
            var success = true;
            var files = SelectFiles();

            if (files.Length > 0)
            {
                var filesInfoPath = Path.Combine(Workflow.WorkflowTempFolder,
                    $"FilesInfo_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}.xml");

                var xdoc = new XDocument(new XElement("Files"));
                foreach (var file in files)
                {
                    try
                    {
                        if (xdoc.Root != null)
                        {
                            const string dateFormat = @"MM\/dd\/yyyy HH:mm.ss";
                            var fileInfo = new FileInfo(file.Path);
                            var xfile = new XElement("File",
                                new XAttribute("path", file.Path),
                                new XAttribute("name", file.FileName),
                                new XAttribute("renameToOrName", file.RenameToOrName),

                                new XAttribute("createdOn", fileInfo.CreationTime.ToString(dateFormat)),
                                new XAttribute("lastWriteOn", fileInfo.LastWriteTime.ToString(dateFormat)),
                                new XAttribute("lastAcessOn", fileInfo.LastAccessTime.ToString(dateFormat)),

                                new XAttribute("isReadOnly", fileInfo.IsReadOnly),
                                new XAttribute("length", fileInfo.Length),
                                new XAttribute("attributes", fileInfo.Attributes)
                                );

                            foreach (var tag in file.Tags)
                            {
                                xfile.SetAttributeValue(tag.Key, tag.Value);
                            }

                            xdoc.Root.Add(xfile);
                        }
                        InfoFormat("File information of the file {0} generated.", file.Path);

                        if (!atLeastOneSucceed)
                        {
                            atLeastOneSucceed = true;
                        }
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        ErrorFormat("An error occured while generating file information of the file {0}", e, file.Path);
                        success = false;
                    }
                }
                xdoc.Save(filesInfoPath);
                Files.Add(new FileInf(filesInfoPath, Id));
            }
            return success;
        }
    }
}