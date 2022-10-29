using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using System.Xml.Xsl;
using Wexflow.Core;

namespace Wexflow.Tasks.Xslt
{
    public class Xslt : Task
    {
        public string XsltPath { get; private set; }
        public string Version { get; private set; }
        public bool RemoveWexflowProcessingNodes { get; private set; }
        public string Extension { get; private set; }
        public string OutputFormat { get; private set; }
        public Xslt(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            XsltPath = GetSetting("xsltPath");
            RemoveWexflowProcessingNodes = bool.Parse(GetSetting("removeWexflowProcessingNodes", "true"));
            Extension = GetSetting("extension", "xml");
            OutputFormat = GetSetting("outputFormat", "{0}_{1:yyyy-MM-dd-HH-mm-ss-fff}.{2}");
        }

        public override TaskStatus Run()
        {
            Info("Transforming files...");

            bool success = true;
            bool atLeastOneSucceed = false;

            foreach (FileInf file in SelectFiles())
            {
                var destPath = Path.Combine(Workflow.WorkflowTempFolder,
                    string.Format(OutputFormat, Path.GetFileNameWithoutExtension(file.FileName), DateTime.Now, Extension));

                try
                {
                    Version = XDocument.Load(XsltPath).Root.Attribute("version").Value;

                    switch (Version)
                    {
                        case "1.0":
                            var xslt = new XslCompiledTransform();
                            xslt.Load(XsltPath);
                            xslt.Transform(file.Path, destPath);
                            InfoFormat("File transformed (XSLT 1.0): {0} -> {1}", file.Path, destPath);
                            Files.Add(new FileInf(destPath, Id));
                            break;
                        case "2.0":
                            Error("XSLT 2.0 is not supported on this platform.");
                            return new TaskStatus(Status.Error, false);
                        case "3.0":
                            Error("XSLT 3.0 is not supported on this platform.");
                            return new TaskStatus(Status.Error, false);
                        default:
                            Error("Error in version option. Available options: 1.0");
                            return new TaskStatus(Status.Error, false);
                    }

                    // Set renameTo and tags from /*//<WexflowProcessing>//<File> nodes
                    // Remove /*//<WexflowProcessing> nodes if necessary

                    var xdoc = XDocument.Load(destPath);
                    var xWexflowProcessings = xdoc.Descendants("WexflowProcessing").ToArray();
                    foreach (var xWexflowProcessing in xWexflowProcessings)
                    {
                        var xFiles = xWexflowProcessing.Descendants("File");
                        foreach (var xFile in xFiles)
                        {
                            try
                            {
                                var taskId = int.Parse(xFile.Attribute("taskId").Value);
                                string fileName = xFile.Attribute("name").Value;
                                var xRenameTo = xFile.Attribute("renameTo");
                                string renameTo = xRenameTo != null ? xRenameTo.Value : string.Empty;
                                var tags = (from xTag in xFile.Attributes()
                                            where xTag.Name != "taskId" && xTag.Name != "name" && xTag.Name != "renameTo" && xTag.Name != "path" && xTag.Name != "renameToOrName"
                                            select new Tag(xTag.Name.ToString(), xTag.Value)).ToList();

                                var fileToEdit = (from f in Workflow.FilesPerTask[taskId]
                                                  where f.FileName.Equals(fileName)
                                                  select f).FirstOrDefault();

                                if (fileToEdit != null)
                                {
                                    fileToEdit.RenameTo = renameTo;
                                    fileToEdit.Tags.AddRange(tags);
                                    InfoFormat("File edited: {0}", fileToEdit.ToString());
                                }
                                else
                                {
                                    ErrorFormat("Cannot find the File: {{fileName: {0}, taskId:{1}}}", fileName, taskId);
                                }
                            }
                            catch (ThreadAbortException)
                            {
                                throw;
                            }
                            catch (Exception e)
                            {
                                ErrorFormat("An error occured while editing the file: {0}. Error: {1}", xFile.ToString(), e.Message);
                            }
                        }
                    }

                    if (RemoveWexflowProcessingNodes)
                    {
                        xWexflowProcessings.Remove();
                        xdoc.Save(destPath);
                    }

                    if (!atLeastOneSucceed) atLeastOneSucceed = true;
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    ErrorFormat("An error occured while transforming the file {0}", e, file.Path);
                    success = false;
                }
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
