namespace Wexflow.Core
{
    /// <summary>
    /// Tag.
    /// </summary>
    /// <remarks>
    /// Creates a new tag.
    /// </remarks>
    /// <param name="key">Tag key.</param>
    /// <param name="value">Tag value.</param>
    public class Tag(string key, string value)
    {
        /// <summary>
        /// Tag key.
        /// </summary>
        public string Key { get; private set; } = key;
        /// <summary>
        /// Tag value.
        /// </summary>
        public string Value { get; private set; } = value;
    }
}
