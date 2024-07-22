using System.Runtime.Serialization;

namespace Wexflow.Core.Service.Contracts
{
    [DataContract]
    public class SettingInfo(string name, string value, AttributeInfo[] attributes)
    {
        [DataMember]
        public string Name { get; set; } = name;
        [DataMember]
        public string Value { get; set; } = value;
        [DataMember]
        public AttributeInfo[] Attributes { get; set; } = attributes;
    }
}
