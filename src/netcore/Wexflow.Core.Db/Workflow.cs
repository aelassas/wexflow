namespace Wexflow.Core.Db
{
    public class Workflow
    {
        public const string DocumentName = "workflows";

        public string Xml { get; set; }

        public virtual string GetDbId()
        {
            return "-1";
        }
    }
}
