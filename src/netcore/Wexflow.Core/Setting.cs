namespace Wexflow.Core
{
    /// <summary>
    /// Setting.
    /// </summary>
    public class Setting
    {
        /// <summary>
        /// Setting name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Settings value.
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// Setting attributes.
        /// </summary>
        public Attribute[] Attributes { get; set; }

        /// <summary>
        /// Creates a new setting.
        /// </summary>
        /// <param name="name">Setting name.</param>
        /// <param name="value">Setting value.</param>
        /// <param name="attributes">Setting attributes.</param>
        public Setting(string name, string value, Attribute[] attributes)
        {
            Name = name;
            Value = value;
            Attributes = attributes;
        }
    }
}