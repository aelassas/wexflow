using System;
using Wexflow.Core;
using System.Xml.Linq;
using System.Threading;

namespace Wexflow.Tasks.Wait
{
    public class Wait : Task
    {
        private CancellationTokenSource _cancellationTokenSource;

        public TimeSpan Duration { get; private set; }

        public Wait(XElement xe, Workflow wf) : base(xe, wf)
        {
            Duration = TimeSpan.Parse(GetSetting("duration"));
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public override TaskStatus Run()
        {
            InfoFormat("Waiting for {0} ...", Duration);

            bool success = true;

            try
            {
                _cancellationTokenSource.Token.WaitHandle.WaitOne(Duration);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while waiting. Error: {0}", e.Message);
                success = false;
            }

            var status = Status.Success;

            if (!success)
            {
                status = Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status);
        }

        public override void Stop()
        {
            base.Stop();
            _cancellationTokenSource.Cancel();
        }
    }
}
