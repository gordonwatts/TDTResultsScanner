
using Akavache;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace ScanTDTResults
{
    public class TDTBuildReport
    {
        private Jenkins.Domain.JenkinsBuild _build;

        public TDTBuildReport(Jenkins.Domain.JenkinsBuild _build)
        {
            // TODO: Complete member initialization
            this._build = _build;

            BuildWarningsTotal = 0;
        }

        private bool _parsed = false;
        public int BuildWarningsTotal { get; private set; }
        public int BuildWarningsBoost { get; private set; }

        /// <summary>
        /// Get the build report (console text) and parse it.
        /// </summary>
        private async Task Parse()
        {
            // This is an expense operation.
            if (_parsed)
                return;
            _parsed = true;

            // First, get and cache the web client. If we've cached it, no worries.
            var fullLog = await BlobCache.UserAccount.GetOrFetchObject(_build.ConsoleTextUrl, () => TDTJobAccess.GetAsString(_build.ConsoleTextUrl))
                .FirstAsync();

            // Now, look for warnings during the build. This is, basically, a line-by-line search for things.
            foreach (var line in fullLog.AsLines())
            {
                if (line.Contains(": warning:"))
                {
                    BuildWarningsTotal++;
                    if (line.Contains("boost"))
                    {
                        BuildWarningsBoost++;
                    }
                }
            }

        }

        /// <summary>
        /// Dump out the log of the build.
        /// </summary>
        /// <param name="wr"></param>
        public async Task WriteReport(TextWriter wr, string indent = "")
        {
            await Parse();
            wr.WriteLine("{1}Number of build warnings: {0}", BuildWarningsTotal, indent);
            wr.WriteLine("{1}  Number due to boost: {0}", BuildWarningsBoost, indent);
        }
    }
}
