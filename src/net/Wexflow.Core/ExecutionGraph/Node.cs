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
        /// Depth.
        /// </summary>
        public int Depth { get; set; }

        /// <summary>
        /// Creates a new node.
        /// </summary>
        /// <param name="id">Node id.</param>
        /// <param name="parentId">Node parent id.</param>
        /// <param name="depth">Depth.</param>
        public Node(int id, int parentId, int depth = 0)
        {
            Id = id;
            ParentId = parentId;
            Depth = depth;
        }
    }
}
