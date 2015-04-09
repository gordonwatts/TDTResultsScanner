using Jenkins.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ScanTDTResults
{
    public class TDTJobRun
    {
        /// <summary>
        /// The build for this run
        /// </summary>
        private JenkinsBuild _build;

        /// <summary>
        /// Parameters for the build
        /// </summary>
        public Dictionary<string, string> Parameters { get; private set; }

        /// <summary>
        /// Regular expression to properly pull appart a URL, pulled from
        /// http://www.cambiaresearch.com/articles/46/parsing-urls-with-regular-expressions-and-the-regex-object
        /// </summary>
        private Regex _urlParser = new Regex(@"^(?<s1>(?<s0>[^:/\?#]+):)?(?<a1>"
                                  + @"//(?<a0>[^/\?#]*))?(?<p0>[^\?#]*)"
                                  + @"(?<q1>\?(?<q0>[^#]*))?"
                                  + @"(?<f1>#(?<f0>.*))?");

        public TDTJobRun(JenkinsBuild jenkinsBuild)
        {
            this._build = jenkinsBuild;

            var m = _urlParser.Match(jenkinsBuild.Url);
            if (!m.Success)
            {
                throw new ArgumentException("Unknown url format: {0}", jenkinsBuild.Url);
            }

            var queryParams = m.Groups["p0"].Value.Split('/').Where(s => s.Contains(",")).Last().Split(',');
            Parameters = new Dictionary<string, string>();
            foreach (var pair in queryParams)
            {
                var keyValue = pair.Split('=');
                Parameters[keyValue[0]] = keyValue[1];
            }
        }

        /// <summary>
        /// What release of root core was used in the job?
        /// </summary>
        public string RootCoreRelease
        {
            get { return Parameters["RELEASE"]; }
        }

        /// <summary>
        /// The input file that was used.
        /// </summary>
        public string InputFile
        {
            get { return Parameters["INPUTFILE"]; }
        }

        private Regex _recoReleaseFinder = new Regex(@"_r(\d+)_");

        /// <summary>
        /// The ami tag
        /// </summary>
        public string AmiRecoReleaseTag
        {
            get
            {
                var p = Parameters["INPUTFILE"];
                var m = _recoReleaseFinder.Match(p);
                if (!m.Success)
                    return "";
                return m.Groups[1].Value;
            }
        }

        /// <summary>
        /// Get the build report for this guy
        /// </summary>
        public TDTBuildReport BuildReport
        {
            get
            {
                return new TDTBuildReport(_build);
            }
        }
    }
}
