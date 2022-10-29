using System;
using Wexflow.Core;
using System.Xml.Linq;
using System.IO;
using System.Threading;

namespace Wexflow.Tasks.FilesInfo
{
    public class FilesInfo : Task
    {
        public FilesInfo(XElement xe, Workflow wf)
            : base(xe, wf)
        {
        }

        public override TaskStatus Run()
        {
            Info("Generating files informations...");

            bool success = true;
            bool atLeastOneSucceed = false;

            var files = SelectFiles();

            if (files.Length > 0)
            {
                var filesInfoPath = Path.Combine(Workflow.WorkflowTempFolder,
                    string.Format("FilesInfo_{0:yyyy-MM-dd-HH-mm-ss-fff}.xml", DateTime.Now));

                var xdoc = new XDocument(new XElement("Files"));
                foreach (FileInf file in files)
                {
                    try
                    {
                        if (xdoc.Root != null)
                        {
                            const string dateFormat = @"MM\/dd\/yyyy HH:mm.ss";
                            FileInfo fileInfo = new FileInfo(file.Path);
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
                        
                        if (!atLeastOneSucceed) atLeastOneSucceed = true;
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
    }
}