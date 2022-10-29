namespace Wexflow.Core.Db
{
    public class UserWorkflow
    {
        public static readonly string DocumentName = "userworkflow";

        public string UserId { get; set; }
        public string WorkflowId { get; set; }
    }
}
