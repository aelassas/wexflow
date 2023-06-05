using Saxon.Api;
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
        public string SmbComputerName { get; private set; }
        public string SmbDomain { get; private set; }
        public string SmbUsername { get; private set; }
        public string SmbPassword { get; private set; }

        public Xslt(XElement xe, Workflow wf) : base(xe, wf)
        {
            XsltPath = GetSetting("xsltPath");
            RemoveWexflowProcessingNodes = bool.Parse(GetSetting("removeWexflowProcessingNodes", "true"));
            Extension = GetSetting("extension", "xml");
            OutputFormat = GetSetting("outputFormat", "{0}_{1:yyyy-MM-dd-HH-mm-ss-fff}.{2}");
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Transforming files...");

            bool success = true;
            bool atLeastOneSucceed = false;

            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        success = TransformFiles(ref atLeastOneSucceed);
                    }
                }
                else
                {
                    success = TransformFiles(ref atLeastOneSucceed);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while processing files.", e);
                success = false;
            }


            Status status = Status.Success;

            if (!success && atLeastOneSucceed)
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

        private bool TransformFiles(ref bool atLeastOneSucceed)
        {
            bool success = true;
            foreach (FileInf file in SelectFiles())
            {
                string destPath = Path.Combine(Workflow.WorkflowTempFolder,
                    string.Format(OutputFormat, Path.GetFileNameWithoutExtension(file.FileName), DateTime.Now, Extension));

                try
                {
                    Version = XDocument.Load(XsltPath).Root.Attribute("version").Value;

                    switch (Version)
                    {
                        case "1.0":
                            XslCompiledTransform xslt = new XslCompiledTransform();
                            xslt.Load(XsltPath);
                            xslt.Transform(file.Path, destPath);
                            InfoFormat("File transformed (XSLT 1.0): {0} -> {1}", file.Path, destPath);
                            Files.Add(new FileInf(destPath, Id));
                            break;
                        case "2.0":
                            FileInfo xsl = new FileInfo(XsltPath);
                            FileInfo input = new FileInfo(file.Path);
                            FileInfo output = new FileInfo(destPath);

                            // Compile stylesheet
                            Processor processor = new Processor();
                            XsltCompiler compiler = processor.NewXsltCompiler();
                            XsltExecutable executable = compiler.Compile(new Uri(xsl.FullName));

                            // Do transformation to a destination
                            DomDestination destination = new DomDestination();
                            using (FileStream inputStream = input.OpenRead())
                            {
                                XsltTransformer transformer = executable.Load();
                                transformer.SetInputStream(inputStream, new Uri(input.DirectoryName));
                                transformer.Run(destination);
                            }

                            // Save result to a file (or whatever else you wanna do)
                            destination.XmlDocument.Save(output.FullName);

                            InfoFormat("File transformed (XSLT 2.0): {0} -> {1}", file.Path, destPath);
                            Files.Add(new FileInf(destPath, Id));
                            break;
                        case "3.0":
                            FileInfo xsl3 = new FileInfo(XsltPath);
                            FileInfo input3 = new FileInfo(file.Path);
                            FileInfo output3 = new FileInfo(destPath);

                            Processor processor3 = new Processor(false);
                            XsltCompiler compiler3 = processor3.NewXsltCompiler();
                            XsltExecutable stylesheet = compiler3.Compile(new Uri(xsl3.FullName));
                            Serializer serializer = processor3.NewSerializer();
                            serializer.SetOutputFile(output3.FullName);

                            using (FileStream inputStream = input3.OpenRead())
                            {
                                Xslt30Transformer transformer3 = stylesheet.Load30();
                                transformer3.Transform(inputStream, serializer);
                                serializer.Close();
                            }

                            InfoFormat("File transformed (XSLT 3.0): {0} -> {1}", file.Path, destPath);
                            Files.Add(new FileInf(destPath, Id));
                            break;
                        default:
                            Error("Error in version option. Available options: 1.0, 2.0 or 3.0");
                            success = false;
                            break;
                    }

                    // Set renameTo and tags from /*//<WexflowProcessing>//<File> nodes
                    // Remove /*//<WexflowProcessing> nodes if necessary

                    XDocument xdoc = XDocument.Load(destPath);
                    XElement[] xWexflowProcessings = xdoc.Descendants("WexflowProcessing").ToArray();
                    foreach (XElement xWexflowProcessing in xWexflowProcessings)
                    {
                        System.Collections.Generic.IEnumerable<XElement> xFiles = xWexflowProcessing.Descendants("File");
                        foreach (XElement xFile in xFiles)
                        {
                            try
                            {
                                int taskId = int.Parse(xFile.Attribute("taskId").Value);
                                string fileName = xFile.Attribute("name").Value;
                                XAttribute xRenameTo = xFile.Attribute("renameTo");
                                string renameTo = xRenameTo != null ? xRenameTo.Value : string.Empty;
                                System.Collections.Generic.List<Tag> tags = (from xTag in xFile.Attributes()
                                                                             where xTag.Name != "taskId" && xTag.Name != "name" && xTag.Name != "renameTo" && xTag.Name != "path" && xTag.Name != "renameToOrName"
                                                                             select new Tag(xTag.Name.ToString(), xTag.Value)).ToList();

                                FileInf fileToEdit = (from f in Workflow.FilesPerTask[taskId]
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
            return success;
        }
    }
}
