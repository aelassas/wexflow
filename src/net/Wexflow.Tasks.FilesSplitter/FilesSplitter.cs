using System;
using Wexflow.Core;
using System.Xml.Linq;
using System.IO;
using System.Threading;

namespace Wexflow.Tasks.FilesSplitter
{
    public class FilesSplitter : Task
    {
        public int ChunkSize { get; private set; }
        public string SmbComputerName { get; private set; }
        public string SmbDomain { get; private set; }
        public string SmbUsername { get; private set; }
        public string SmbPassword { get; private set; }

        public FilesSplitter(XElement xe, Workflow wf) : base(xe, wf)
        {
            ChunkSize = int.Parse(GetSetting("chunkSize"));
            SmbComputerName = GetSetting("smbComputerName");
            SmbDomain = GetSetting("smbDomain");
            SmbUsername = GetSetting("smbUsername");
            SmbPassword = GetSetting("smbPassword");
        }

        public override TaskStatus Run()
        {
            Info("Splitting files into chunks...");

            var success = true;
            var atLeastOneSucceed = false;

            try
            {
                if (!string.IsNullOrEmpty(SmbComputerName) && !string.IsNullOrEmpty(SmbUsername) && !string.IsNullOrEmpty(SmbPassword))
                {
                    using (NetworkShareAccesser.Access(SmbComputerName, SmbDomain, SmbUsername, SmbPassword))
                    {
                        success = SplitFiles(ref atLeastOneSucceed);
                    }
                }
                else
                {
                    success = SplitFiles(ref atLeastOneSucceed);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while splitting files.", e);
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

        private bool SplitFiles(ref bool atLeastOneSucceed)
        {
            var success = true;
            var files = SelectFiles();

            if (files.Length > 0)
            {
                foreach (FileInf file in files)
                {
                    try
                    {
                        int index = 0;

                        const int bufferSize = 20 * 1024;
                        byte[] buffer = new byte[bufferSize];

                        using (Stream input = File.OpenRead(file.Path))
                        {
                            while (input.Position < input.Length)
                            {
                                string chunkPath = Path.Combine(Workflow.WorkflowTempFolder, file.FileName + "_" + (index + 1));
                                using (Stream output = File.Create(chunkPath))
                                {
                                    int remaining = ChunkSize, bytesRead;
                                    while (remaining > 0 && (bytesRead = input.Read(buffer, 0, Math.Min(remaining, bufferSize))) > 0)
                                    {
                                        output.Write(buffer, 0, bytesRead);
                                        remaining -= bytesRead;
                                    }
                                    Files.Add(new FileInf(chunkPath, Id));
                                }
                                index++;
                                //Thread.Sleep(500); // experimental; perhaps try it
                            }
                        }

                        InfoFormat("The file {0} was splitted into {1} chunks.", file.Path, index + 1);

                        if (!atLeastOneSucceed) atLeastOneSucceed = true;
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        ErrorFormat("An error occured while splitting the file {0}", e, file.Path);
                        success = false;
                    }
                }
            }
            return success;
        }
    }
}