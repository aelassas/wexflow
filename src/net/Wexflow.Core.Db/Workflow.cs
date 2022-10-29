using System;

namespace Wexflow.Core.Db
{
    public class Workflow
    {
        public static readonly string DocumentName = "workflows";

        public string Xml { get; set; }

        public virtual string GetDbId()
        {
            return "-1";
        }
    }
}
