using System;
using System.Linq;

namespace AutoSats.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Removes <paramref name="maxCount"/> of the leading occurrences of a set of characters specified in an array from the current string.
        /// </summary>
        public static string TrimStart(this string s, int maxCount, params char[] chars)
        {
            var i = 0;
            var max = Math.Min(s.Length, maxCount);

            for (; i < max; i++)
            {
                if (!chars.Contains(s[i]))
                {
                    break;
                }
            }

            return s.Substring(i);
        }
    }
}
