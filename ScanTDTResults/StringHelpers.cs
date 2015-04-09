using System.Collections.Generic;

namespace ScanTDTResults
{
    static class StringHelpers
    {
        /// <summary>
        /// Returns a string split up into lines (looking for end-of-line characters)
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<string> AsLines(this string source)
        {
            int index = 0;
            while (true)
            {
                var newLineLoc = source.IndexOf('\n', index);
                if (newLineLoc < 0)
                {
                    var line = source.Substring(index);
                    if (line.Length > 0)
                        yield return line;
                    break;
                }
                else
                {
                    var line = source.Substring(index, newLineLoc - index);
                    yield return line;
                    index = newLineLoc + 1;
                }
            }
        }
    }
}
