using Wexflow.Core.ExecutionGraph;

namespace Wexflow.Server.Contracts.Workflow
{
    public class Workflow
    {
        public WorkflowInfo WorkflowInfo { get; set; }
        public TaskInfo[] Tasks { get; set; }
        public Graph ExecutionGraph { get; set; }
    }
}
