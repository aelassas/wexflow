using System;
using System.Globalization;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.Now
{
    public class Now : Task
    {
        public string Cultre { get; private set; }
        public string Format { get; private set; }

        public Now(XElement xe, Workflow wf) : base(xe, wf)
        {
            Cultre = GetSetting("culture");
            Format = GetSetting("format");
        }

        public override TaskStatus Run()
        {
            Info("Getting current date...");
            string value = string.Empty;
            bool succeeded = false;
            try
            {
                value = string.Format(new CultureInfo(Cultre), "{0:"+Format+"}", DateTime.Now);
                InfoFormat("The value is: {0}", value);
                succeeded = true;
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while retrieving current date. Error: {0}", e.Message);
            }
            Info("Task finished.");
            return new TaskStatus(succeeded ? Status.Success : Status.Error, value);
        }
    }
}