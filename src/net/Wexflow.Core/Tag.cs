namespace Wexflow.Core
{
    /// <summary>
    /// Tag.
    /// </summary>
    public class Tag
    {
        /// <summary>
        /// Tag key.
        /// </summary>
        public string Key { get; private set; }
        /// <summary>
        /// Tag value.
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Creates a new tag.
        /// </summary>
        /// <param name="key">Tag key.</param>
        /// <param name="value">Tag value.</param>
        public Tag(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}
