using System.Collections.Generic;
using System.Linq;

namespace Wexflow.Core.ExecutionGraph
{
    /// <summary>
    /// Graph event.
    /// </summary>
    public class GraphEvent
    {
        /// <summary>
        /// Nodes.
        /// </summary>
        public Node[] Nodes { get; set; }

        /// <summary>
        /// Creates a new graph event.
        /// </summary>
        /// <param name="nodes">Nodes.</param>
        public GraphEvent(IEnumerable<Node> nodes)
        {
            if (nodes != null)
            {
                Nodes = nodes.ToArray();
            }
        }
    }
}
