using System;
using System.Text.RegularExpressions;

namespace Wexflow.Tasks.SshCmd
{
    public static partial class StringExt
    {
        public static string StringAfter(this string str, string substring)
        {
            var index = str.IndexOf(substring, StringComparison.Ordinal);

            return index >= 0 ? str[(index + substring.Length)..] : string.Empty;
        }

        public static string[] GetLines(this string str)
        {
            return MyRegex().Split(str);
        }

        [GeneratedRegex("\r\n|\r|\n")]
        private static partial Regex MyRegex();
    }
}
