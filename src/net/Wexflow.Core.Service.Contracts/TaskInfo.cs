using System.Runtime.Serialization;

namespace Wexflow.Core.Service.Contracts
{
    [DataContract]
    public class TaskInfo
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public bool IsEnabled { get; set; }
        [DataMember]
        public SettingInfo[] Settings { get; set; }

        public TaskInfo(int id, string name, string desc, bool isEnabled, SettingInfo[] settings)
        {
            Id = id;
            Name = name;
            Description = desc;
            IsEnabled = isEnabled;
            Settings = settings;
        }
    }
}
