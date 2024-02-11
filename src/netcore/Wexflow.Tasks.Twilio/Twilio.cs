using System;
using System.Threading;
using System.Xml.Linq;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Wexflow.Core;

namespace Wexflow.Tasks.Twilio
{
    public class Twilio : Task
    {
        public string AccountSid { get; }
        public string AuthToken { get; }
        public string From { get; }
        public string To { get; }
        public string Message { get; }

        public Twilio(XElement xe, Workflow wf) : base(xe, wf)
        {
            AccountSid = GetSetting("accountSid");
            AuthToken = GetSetting("authToken");
            From = GetSetting("from");
            To = GetSetting("to");
            Message = GetSetting("message");
        }

        public override TaskStatus Run()
        {
            Info("Sending SMS...");

            var success = true;

            try
            {
                TwilioClient.Init(AccountSid, AuthToken);

                var message = MessageResource.Create(
                    body: Message,
                    from: new PhoneNumber(From),
                    to: new PhoneNumber(To)
                );

                if (message.ErrorCode != null)
                {
                    ErrorFormat("An error occured while sending the SMS: ErrorCode={0}, ErrorMessage={1}", message.ErrorCode, message.ErrorMessage);
                    success = false;
                }
                else
                {
                    InfoFormat("SMS sent: message.Sid={0}", message.Sid);
                }
                WaitOne();
            }
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while sending SMS.", e);
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
    }
}
