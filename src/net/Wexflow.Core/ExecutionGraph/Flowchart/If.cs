using System.Collections.Generic;
using System.Linq;

namespace Wexflow.Core.ExecutionGraph.Flowchart
{
    /// <summary>
    /// If flowchart node.
    /// </summary>
    public class If : Node
    {
        /// <summary>
        /// If Id.
        /// </summary>
        public int IfId { get; set; }
        /// <summary>
        /// Do Nodes.
        /// </summary>
        public Node[] DoNodes { get; set; }
        /// <summary>
        /// Else nodes.
        /// </summary>
        public Node[] ElseNodes { get; set; }

        /// <summary>
        /// Creates a new If flowchart node.
        /// </summary>
        /// <param name="id">Id.</param>
        /// <param name="parentId">Parent Id.</param>
        /// <param name="ifId">If Id.</param>
        /// <param name="doNodes">Do nodes.</param>
        /// <param name="elseNodes">Else nodes.</param>
        public If(int id, int parentId, int ifId, IEnumerable<Node> doNodes, IEnumerable<Node> elseNodes) : base(id, parentId)
        {
            IfId = ifId;
            if (doNodes != null)
            {
                DoNodes = doNodes.ToArray();
            }
            if (elseNodes != null)
            {
                ElseNodes = elseNodes.ToArray();
            }
        }
    }
}
