
using Jenkins.Core;
using Jenkins.Domain;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace ScanTDTResults
{
    /// <summary>
    /// Make access to jenkins easy
    /// </summary>
    public class TDTJobAccess
    {
        /// <summary>
        /// The client we will use to download things from the web.
        /// </summary>
        private static IJenkinsRestClient _client = null;

        /// <summary>
        /// Get the jenkins lookup client.
        /// </summary>
        /// <returns></returns>
        internal static IJenkinsRestClient GetClient()
        {
            return _client;
        }

        private static BasicAuthJenkinsClient _webClient = null;

        /// <summary>
        /// Load up the access. NOTE: your api key should be in the windows credential store for this server!!!
        /// Initialize Jenkin's library access for everything.
        /// </summary>
        /// <remarks>
        /// To put the username and password in, open the "Credential Manager" control panel.
        /// Then for window's credentials, under "Generic credentials" make an entry with:
        /// Internet or network address: jenks-higgs.phys.washington.edu:8080
        /// Username: your username
        /// Password: the api key from the jenkins server.
        /// </remarks>
        public TDTJobAccess()
        {
            var factory = new JenkinsRestFactory();
            var cred = WebGenericCredentialsLib.CredAccess.LookupUserPass("jenks-higgs.phys.washington.edu:8080");
            if (cred != null)
            {
                _webClient = new BasicAuthJenkinsClient(cred.Item1, cred.Item2);
                factory.SetRestClient(_webClient);
            }

            _client = factory.GetClient();
        }

        /// <summary>
        /// Gets a list of the runs from the last build.
        /// </summary>
        /// <returns></returns>
        public async Task<JenkinsBuild> GetLastBuild()
        {
            // Get the note
            var server = await _client.GetServerAsync("http://jenks-higgs.phys.washington.edu:8080");

            // Find the job we want.
            var job = server.Node.Jobs.Where(j => j.Name == "TDTCompatabilityTesting").FirstOrDefault();
            if (job == null)
            {
                throw new InvalidOperationException("Unable to find the TDTCompatibilityTesting job");
            }

            // And get the last good build
            var jobInfo = await _client.GetJobAsync(job.RestUrl);

            var lastBuild = await _client.GetBuildAsync(jobInfo.LastBuild.RestUrl);

            return lastBuild;
        }

        /// <summary>
        /// Return a list of the runs for the build.
        /// </summary>
        /// <param name="lastBuild"></param>
        /// <returns></returns>
        public async Task<TDTJobRun[]> GetRunsForBuild(JenkinsBuild build)
        {
            var allasTasks = build.Runs
                .Where(r => r.Number == build.Number)
                .Select(async r => await _client.GetBuildAsync(r.RestUrl))
                .Select(async binfo => new TDTJobRun(await binfo))
                .ToArray();

            await Task.WhenAll(allasTasks);
            return allasTasks.Select(t => t.Result).ToArray();
        }

        /// <summary>
        /// Get the data as a string from the given url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        internal static IObservable<string> GetAsString(string url)
        {
            return Observable.FromAsync(() => _webClient.DownloadStringTaskAsync(url));
        }

        internal static IObservable<byte[]> GetAsBytes(string url)
        {
            return Observable.FromAsync(() => _webClient.DownloadBytesTaskAsync(url));
        }
    }
}
