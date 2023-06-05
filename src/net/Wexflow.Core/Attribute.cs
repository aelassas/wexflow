namespace Wexflow.Core
{
    /// <summary>
    /// Attribute.
    /// </summary>
    public class Attribute
    {
        /// <summary>
        /// Attribute name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Attribute value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Creates a new instance of Attribute.
        /// </summary>
        /// <param name="name">Attribute name.</param>
        /// <param name="value">Attribute value.</param>
        public Attribute(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}