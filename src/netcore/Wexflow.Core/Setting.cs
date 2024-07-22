namespace Wexflow.Core
{
    /// <summary>
    /// Setting.
    /// </summary>
    /// <remarks>
    /// Creates a new setting.
    /// </remarks>
    /// <param name="name">Setting name.</param>
    /// <param name="value">Setting value.</param>
    /// <param name="attributes">Setting attributes.</param>
    public class Setting(string name, string value, Attribute[] attributes)
    {
        /// <summary>
        /// Setting name.
        /// </summary>
        public string Name { get; set; } = name;
        /// <summary>
        /// Settings value.
        /// </summary>
        public string Value { get; set; } = value;
        /// <summary>
        /// Setting attributes.
        /// </summary>
        public Attribute[] Attributes { get; set; } = attributes;
    }
}