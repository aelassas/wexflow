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
            Workflow.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            InfoFormat("Waiting for {0} ...", Duration);

            var success = true;

            try
            {
                var duration = Duration.TotalMilliseconds;
                double t = 0;
                while (t < duration)
                {
                    Workflow.CancellationTokenSource.Token.ThrowIfCancellationRequested();

                    // Sleep in 1-second intervals (or less if near end)
                    var remaining = duration - t;
                    var waitTime = TimeSpan.FromMilliseconds(Math.Min(1000, remaining));

                    // Wait for either cancellation or timeout
                    var canceled = Workflow.CancellationTokenSource.Token.WaitHandle.WaitOne(waitTime);
                    if (canceled)
                    {
                        throw new OperationCanceledException();
                    }

                    t += waitTime.TotalMilliseconds;

                    if (!Workflow.CancellationTokenSource.Token.IsCancellationRequested)
                    {
                        WaitOne();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while waiting. Error: {0}", e.Message);
                success = false;
            }

            var status = success ? Status.Success : Status.Error;
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
