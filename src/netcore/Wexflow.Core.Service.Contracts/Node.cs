using System.Runtime.Serialization;

namespace Wexflow.Core.Service.Contracts
{
    [DataContract]
    public class Node(string id, string name, string parentId)
    {
        [DataMember]
        public string Id { get; private set; } = id;
        [DataMember]
        public string Name { get; private set; } = name;
        [DataMember]
        public string ParentId { get; private set; } = parentId;
    }
}
