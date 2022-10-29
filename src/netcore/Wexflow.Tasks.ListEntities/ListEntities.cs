using System.Collections.Generic;
using Wexflow.Core;
using System.Xml.Linq;

namespace Wexflow.Tasks.ListEntities
{
    public class ListEntities:Task
    {
        public ListEntities(XElement xe, Workflow wf)
            : base(xe, wf)
        {
        }

        public override TaskStatus Run()
        {
            Info("Listing entities...");
            
            foreach (List<Entity> entities in Workflow.EntitiesPerTask.Values)
            {
                foreach (Entity entity in entities)
                {
                    InfoFormat("{{taskId: {0}, entity: {1}}}", entity.TaskId, entity);
                }
            }

            Info("Task finished.");
            return new TaskStatus(Status.Success, false);
        }
    }
}
