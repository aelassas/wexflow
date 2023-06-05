using System.Collections.Generic;
using System.Linq;

namespace Wexflow.Core.ExecutionGraph.Flowchart
{
    /// <summary>
    /// While flowchart node.
    /// </summary>
    public class While : Node
    {
        /// <summary>
        /// While Id.
        /// </summary>
        public int WhileId { get; set; }
        /// <summary>
        /// Nodes.
        /// </summary>
        public Node[] Nodes { get; set; }

        /// <summary>
        /// Creates a new While flowchart node.
        /// </summary>
        /// <param name="id">Id.</param>
        /// <param name="parentId">Parent Id.</param>
        /// <param name="whileId">While Id.</param>
        /// <param name="nodes">Nodes.</param>
        public While(int id, int parentId, int whileId, IEnumerable<Node> nodes) : base(id, parentId)
        {
            WhileId = whileId;
            if (nodes != null)
            {
                Nodes = nodes.ToArray();
            }
        }
    }
}
