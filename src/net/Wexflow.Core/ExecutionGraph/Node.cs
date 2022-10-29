namespace Wexflow.Core.ExecutionGraph
{
    /// <summary>
    /// Node.
    /// </summary>
    public class Node
    {
        /// <summary>
        /// Node Id.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Node parent Id.
        /// </summary>
        public int ParentId { get; set; }

        /// <summary>
        /// Creates a new node.
        /// </summary>
        /// <param name="id">Node id.</param>
        /// <param name="parentId">Node parent id.</param>
        public Node(int id, int parentId)
        {
            Id = id;
            ParentId = parentId;
        }
    }
}
