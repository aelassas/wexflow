using System.Collections.Generic;
using System.Linq;

namespace Wexflow.Core.ExecutionGraph.Flowchart
{
    /// <summary>
    /// Switch flowchart node.
    /// </summary>
    public class Switch : Node
    {
        /// <summary>
        /// Switch id.
        /// </summary>
        public int SwitchId { get; set; }
        /// <summary>
        /// Cases.
        /// </summary>
        public Case[] Cases { get; set; }
        /// <summary>
        /// Default case.
        /// </summary>
        public Node[] Default { get; set; }

        /// <summary>
        /// Creates a new Switch flowchart node.
        /// </summary>
        /// <param name="id">Id.</param>
        /// <param name="parentId">Parent Id.</param>
        /// <param name="switchId">Switch Id.</param>
        /// <param name="cases">Cases.</param>
        /// <param name="default">Default case.</param>
        public Switch(int id, int parentId, int switchId, IEnumerable<Case> cases, IEnumerable<Node> @default) : base(id, parentId)
        {
            SwitchId = switchId;
            if (cases != null)
            {
                Cases = cases.ToArray();
            }
            if (@default != null)
            {
                Default = @default.ToArray();
            }
        }
    }
}
