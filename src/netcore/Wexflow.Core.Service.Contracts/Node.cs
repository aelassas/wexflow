using System.Runtime.Serialization;

namespace Wexflow.Core.Service.Contracts
{
    [DataContract]
    public class Node
    {
        [DataMember]
        public string Id { get; private set; }
        [DataMember]
        public string Name { get; private set; }
        [DataMember]
        public string ParentId { get; private set; }

        public Node(string id, string name, string parentId)
        {
            Id = id;
            Name = name;
            ParentId = parentId;
        }
    }
}
