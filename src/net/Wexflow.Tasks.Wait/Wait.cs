using System;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.Wait
{
    public class Wait : Task
    {
        private readonly CancellationTokenSource _cancellationTokenSource;

        public TimeSpan Duration { get; }

        public Wait(XElement xe, Workflow wf) : base(xe, wf)
        {
            Duration = TimeSpan.Parse(GetSetting("duration"));
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public override TaskStatus Run()
        {
            InfoFormat("Waiting for {0} ...", Duration);

            var success = true;

            try
            {
                _ = _cancellationTokenSource.Token.WaitHandle.WaitOne(Duration);
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
