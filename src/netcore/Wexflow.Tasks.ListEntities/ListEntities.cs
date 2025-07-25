﻿using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.ListEntities
{
    public class ListEntities : Task
    {
        public ListEntities(XElement xe, Workflow wf)
            : base(xe, wf)
        {
        }

        public override TaskStatus Run()
        {
            Workflow.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            Info("Listing entities...");

            foreach (var entities in Workflow.EntitiesPerTask.Values)
            {
                foreach (var entity in entities)
                {
                    Workflow.CancellationTokenSource.Token.ThrowIfCancellationRequested();
                    InfoFormat("{{taskId: {0}, entity: {1}}}", entity.TaskId, entity);
                    if (!Workflow.CancellationTokenSource.Token.IsCancellationRequested)
                    {
                        WaitOne();
                    }
                }
            }

            Info("Task finished.");
            return new TaskStatus(Status.Success, false);
        }
    }
}
