using System.Runtime.Serialization;

namespace Wexflow.Core.Service.Contracts
{
    [DataContract]
    public class AttributeInfo(string name, string value)
    {
        [DataMember]
        public string Name { get; set; } = name;
        [DataMember]
        public string Value { get; set; } = value;
    }
}
