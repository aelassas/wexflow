using System;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.FilesSplitter
{
    public class FilesSplitter : Task
    {
        public int ChunkSize { get; }

        public FilesSplitter(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            ChunkSize = int.Parse(GetSetting("chunkSize"));
        }

        public override TaskStatus Run()
        {
            Info("Splitting files into chunks...");

            var success = true;
            var atLeastOneSucceed = false;

            var files = SelectFiles();

            if (files.Length > 0)
            {
                foreach (var file in files)
                {
                    try
                    {
                        var index = 0;

                        const int bufferSize = 20 * 1024;
                        var buffer = new byte[bufferSize];

                        using (var input = File.OpenRead(file.Path))
                        {
                            while (input.Position < input.Length)
                            {
                                var chunkPath = Path.Combine(Workflow.WorkflowTempFolder, $"{file.FileName}_{index + 1}");
                                using (var output = File.Create(chunkPath))
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
                        ErrorFormat("An error occured while splitting the file {0}", e, file.Path);
                        success = false;
                    }
                    finally
                    {
                        WaitOne();
                    }
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