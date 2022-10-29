using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.MessageCorrect
{
    public class MessageCorrect : Task
    {
        public string CheckString { get; private set; }

        public MessageCorrect(XElement xe, Workflow wf) : base(xe, wf)
        {
            CheckString = GetSetting("checkString");
        }

        public override TaskStatus Run()
        {
            try
            {
                var o = SharedMemory["message"];
                string message = o == null ? string.Empty : o.ToString();
                bool result = message.IndexOf(CheckString) >= 0;
                Info("The result is " + result);

                return new TaskStatus(result ? Status.Success : Status.Error, result, message);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
        }
    }
}