using System.Runtime.Serialization;

namespace Wexflow.Core.Service.Contracts
{
    [DataContract]
    public class TaskInfo(int id, string name, string desc, bool isEnabled, SettingInfo[] settings)
    {
        [DataMember]
        public int Id { get; set; } = id;
        [DataMember]
        public string Name { get; set; } = name;
        [DataMember]
        public string Description { get; set; } = desc;
        [DataMember]
        public bool IsEnabled { get; set; } = isEnabled;
        [DataMember]
        public SettingInfo[] Settings { get; set; } = settings;
    }
}
