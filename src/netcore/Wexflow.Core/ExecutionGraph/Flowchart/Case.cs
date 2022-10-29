using System.Collections.Generic;
using System.Linq;

namespace Wexflow.Core.ExecutionGraph.Flowchart
{
    /// <summary>
    /// Case.
    /// </summary>
    public class Case
    {
        /// <summary>
        /// Case value.
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// Case nodes.
        /// </summary>
        public Node[] Nodes { get; set; }

        /// <summary>
        /// Creates a new case.
        /// </summary>
        /// <param name="val">Case value.</param>
        /// <param name="nodes">Case nodes.</param>
        public Case(string val, IEnumerable<Node> nodes)
        {
            Value = val;
            if (nodes != null)
            {
                Nodes = nodes.ToArray();
            }
        }
    }
}
