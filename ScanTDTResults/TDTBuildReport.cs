
using System;
using System.IO;
using System.Linq;
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
            BuildWarningsBoost = 0;
            isPassedHLTAny = 0;
            isPassedL1Any = 0;
        }

        private bool _parsed = false;
        public int BuildWarningsTotal { get; private set; }
        public int BuildWarningsBoost { get; private set; }

        public bool FoundEvents { get; private set; }

        public bool isPassedWorking { get; private set; }
        public int isPassedL1Any { get; private set; }
        public int isPassedHLTAny { get; private set; }

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
            var fullLog = await TDTJobAccess.GetAsString(_build.ConsoleTextUrl);

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

            // Look at the two artifacts. If there aren't two, then this is very bad.
            var isPassedAPath = _build.Artifacts.Where(a => a.DisplayPath == "trigDecsion.root").FirstOrDefault();
            var featureInfoPath = _build.Artifacts.Where(a => a.DisplayPath == "associatedFeatures.root").FirstOrDefault();

            if (isPassedAPath == null || featureInfoPath == null)
            {
                Console.WriteLine("--> Build seems to be missing proper artifact files!");
                return;
            }

            // Check that we saw events
            var isPassedFile = await isPassedAPath.SaveToTempFile(_build, "root");
            var rf = ROOTNET.NTFile.Open(isPassedFile.FullName, "READ");
            var hpassed = rf.Get("eventCounter") as ROOTNET.Interface.NTH1;
            FoundEvents = hpassed.Entries > 0;
            rf.Close();
        }

        /// <summary>
        /// Dump out the log of the build.
        /// </summary>
        /// <param name="wr"></param>
        public async Task WriteReportBuild(TextWriter wr, string indent = "")
        {
            await Parse();
            wr.WriteLine("{1}Number of build warnings: {0}", BuildWarningsTotal, indent);
            wr.WriteLine("{1}  Number due to boost: {0}", BuildWarningsBoost, indent);
        }

        /// <summary>
        /// Generate a report on how this thing actually ran.
        /// </summary>
        /// <param name="textWriter"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        internal async Task WriteReportRun(TextWriter wr, string indent = "")
        {
            await Parse();
            wr.WriteLine("{0}Event Loop Working: {1}", indent, FoundEvents);
            wr.WriteLine("{0}isPassed: {1}", indent, isPassedWorking);
        }
    }
}
