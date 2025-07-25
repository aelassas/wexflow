﻿using System;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.FolderExists
{
    public class FolderExists : Task
    {
        public string Folder { get; }

        public FolderExists(XElement xe, Workflow wf) : base(xe, wf)
        {
            Folder = GetSetting("folder");
        }

        public override TaskStatus Run()
        {
            Workflow.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            Info("Checking folder...");

            bool success;

            try
            {
                Workflow.CancellationTokenSource.Token.ThrowIfCancellationRequested();
                success = System.IO.Directory.Exists(Folder);

                InfoFormat(success ? "The folder {0} exists." : "The folder {0} does not exist.", Folder);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while checking the folder {0}. Error: {1}", Folder, e.Message);
                return new TaskStatus(Status.Error, false);
            }
            finally
            {
                if (!Workflow.CancellationTokenSource.Token.IsCancellationRequested)
                {
                    WaitOne();
                }
            }

            Info("Task finished");

            return new TaskStatus(Status.Success, success);
        }
    }
}
