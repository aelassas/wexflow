namespace Wexflow.Core;

/// <summary>
/// Log level
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// All logs and debug logs.
    /// </summary>
    Debug,
    /// <summary>
    /// All logs without debug logs.
    /// </summary>
    All,
    /// <summary>
    /// Only last workflow log and error logs.
    /// </summary>
    Severely,
    /// <summary>
    /// Only last workflow log.
    /// </summary>
    Minimum,
    /// <summary>
    /// No logs.
    /// </summary>
    None
}