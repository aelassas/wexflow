using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Wexflow.Core.ExecutionGraph;

namespace Wexflow.Server.Contracts.Workflow
{
    public class Workflow
    {
        public WorkflowInfo WorkflowInfo { get; set; }
        public TaskInfo[] Tasks { get; set; }
        public Graph ExecutionGraph { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public Status Status { get; set; }
    }
}
