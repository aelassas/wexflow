namespace Wexflow.Core.ExecutionGraph
{
    /// <summary>
    /// Node.
    /// </summary>
    /// <remarks>
    /// Creates a new node.
    /// </remarks>
    /// <param name="id">Node id.</param>
    /// <param name="parentId">Node parent id.</param>
    public class Node(int id, int parentId)
    {
        /// <summary>
        /// Node Id.
        /// </summary>
        public int Id { get; set; } = id;
        /// <summary>
        /// Node parent Id.
        /// </summary>
        public int ParentId { get; set; } = parentId;
    }
}
