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
        /// <param name="depth">Depth.</param>
        public While(int id, int parentId, int whileId, IEnumerable<Node> nodes, int depth =0) : base(id, parentId)
        {
            WhileId = whileId;
            if (nodes != null)
            {
                Nodes = nodes.ToArray();
            }
            Depth = depth;
        }
    }
}
