namespace Wexflow.Server.Contracts
{
    public class Node
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string ParentId { get; set; }

        public Node(string id, string name, string parentId)
        {
            Id = id;
            Name = name;
            ParentId = parentId;
        }
    }
}
