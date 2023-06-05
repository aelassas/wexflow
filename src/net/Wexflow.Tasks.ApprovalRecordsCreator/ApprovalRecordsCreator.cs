using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.ApprovalRecordsCreator
{
    public class ApprovalRecordsCreator : Task
    {
        public string CreatedBy { get; private set; }

        public ApprovalRecordsCreator(XElement xe, Workflow wf) : base(xe, wf)
        {
            CreatedBy = GetSetting("createdBy");
        }

        public override TaskStatus Run()
        {
            Info("Importing records...");

            bool success = true;
            bool atLeastOneSuccess = false;

            try
            {
                List<string> recordIds = new List<string>();
                FileInf[] files = SelectFiles();

                foreach (FileInf file in files)
                {
                    try
                    {
                        string recordId = Workflow.WexflowEngine.SaveRecordFromFile(file.Path, CreatedBy);

                        if (recordId != "-1")
                        {
                            Info($"Record inserted from file {file.Path}. RecordId: {recordId}");
                            recordIds.Add(recordId);
                            if (!atLeastOneSuccess)
                            {
                                atLeastOneSuccess = true;
                            }
                        }
                        else
                        {
                            Error($"An error occured while inserting a record from the file {file.Path}.");
                            success = false;
                        }
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        ErrorFormat("An error occured while importing the record {0}.", e, file.Path);
                        success = false;
                    }
                }

                string smKey = "ApprovalRecordsCreator.RecordIds";
                if (SharedMemory.ContainsKey(smKey))
                {
                    SharedMemory.Remove(smKey);
                }
                SharedMemory.Add(smKey, recordIds.ToArray());
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while importing records.", e);
                success = false;
            }

            Status status = Status.Success;

            if (!success && atLeastOneSuccess)
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
    }
}
