namespace Wexflow.Core.Db
{
    public class Workflow
    {
        public const string DOCUMENT_NAME = "workflows";

        public string Xml { get; set; }

        public virtual string GetDbId()
        {
            return "-1";
        }
    }
}
