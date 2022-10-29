using System.Runtime.Serialization;

namespace Wexflow.Core.Service.Contracts
{
    [DataContract]
    public class StatusCount
    {
        [DataMember]
        public int PendingCount { get; set; }
        [DataMember]
        public int RunningCount { get; set; }
        [DataMember]
        public int DoneCount { get; set; }
        [DataMember]
        public int FailedCount { get; set; }
        [DataMember]
        public int WarningCount { get; set; }
        [DataMember]
        public int DisabledCount { get; set; }
        [DataMember]
        public int RejectedCount { get; set; }
        [DataMember]
        public int StoppedCount { get; set; }
    }
}
