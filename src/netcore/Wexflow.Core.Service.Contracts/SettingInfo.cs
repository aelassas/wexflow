using System.Runtime.Serialization;

namespace Wexflow.Core.Service.Contracts
{
    [DataContract]
    public class SettingInfo
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Value { get; set; }
        [DataMember]
        public AttributeInfo[] Attributes { get; set; }

        public SettingInfo(string name, string value, AttributeInfo[] attributes)
        {
            Name = name;
            Value = value;
            Attributes = attributes;
        }
    }
}
