namespace Wexflow.Core
{
    /// <summary>
    /// Attribute.
    /// </summary>
    /// <remarks>
    /// Creates a new instance of Attribute.
    /// </remarks>
    /// <param name="name">Attribute name.</param>
    /// <param name="value">Attribute value.</param>
    public class Attribute(string name, string value)
    {
        /// <summary>
        /// Attribute name.
        /// </summary>
        public string Name { get; set; } = name;
        /// <summary>
        /// Attribute value.
        /// </summary>
        public string Value { get; set; } = value;
    }
}