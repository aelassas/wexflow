using System;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

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

            var success = true;
            var atLeastOneSucceed = false;

            var files = SelectFiles();

            if (files.Length > 0)
            {
                var filesInfoPath = Path.Combine(Workflow.WorkflowTempFolder,
                    $"FilesInfo_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}.xml");

                XDocument xdoc = new(new XElement("Files"));
                foreach (var file in files)
                {
                    try
                    {
                        if (xdoc.Root != null)
                        {
                            const string dateFormat = @"MM\/dd\/yyyy HH:mm.ss";
                            FileInfo fileInfo = new(file.Path);
                            XElement xfile = new("File",
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
                    catch (ThreadInterruptedException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        ErrorFormat("An error occured while generating file information of the file {0}", e, file.Path);
                        success = false;
                    }
                    finally
                    {
                        WaitOne();
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