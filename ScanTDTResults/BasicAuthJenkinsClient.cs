using Akavache;
using Jenkins.Core;
using System;
using System.Net;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanTDTResults
{
    /// <summary>
    /// A Jenkins rest client that uses basic athentications
    /// </summary>
    class BasicAuthJenkinsClient : IJenkinsWebClient
    {
        public BasicAuthJenkinsClient(string username, string apiKey)
        {
            this.Username = username;
            this.ApiKey = apiKey;
        }

        /// <summary>
        /// Download a string, pulling it from a cache first if need be.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<string> DownloadStringTaskAsync(string url)
        {
            return await BlobCache.UserAccount.GetOrFetchObject(url, () => DownloadStringFromWeb(url))
                .FirstAsync();
        }

        /// <summary>
        /// Pull the string from the web directly.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private Task<string> DownloadStringFromWeb(string url)
        {
            using (var wc = new WebClient())
            {
                wc.Headers.Add("Authorization",
                    "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", Username, ApiKey))));
                return wc.DownloadStringTaskAsync(url);
            }
        }

        /// <summary>
        /// Upload a string
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public Task<string> UploadStringTaskAsync(string url, string data)
        {
            using (var wc = new WebClient())
            {
                return wc.UploadStringTaskAsync(url, data);
            }
        }

        public string ApiKey { get; private set; }

        public string Username { get; private set; }

        /// <summary>
        /// Return the bytes for this file.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<byte[]> DownloadBytesTaskAsync(string url)
        {
            var cachedObject = BlobCache.UserAccount.Get(url)
                .Catch(Observable.Empty<byte[]>());

            var fetchedObject = Observable.FromAsync(() => DownloadBytesFromWeb(url))
                .SelectMany(data => BlobCache.UserAccount.Insert(url, data).Select(_ => data));

            return await cachedObject.Concat(fetchedObject).Take(1).FirstAsync();
        }

        /// <summary>
        /// Get a resource from the server as bytes
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private Task<byte[]> DownloadBytesFromWeb(string url)
        {
            using (var wc = new WebClient())
            {
                wc.Headers.Add("Authorization",
                    "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", Username, ApiKey))));
                return wc.DownloadDataTaskAsync(url);
            }
        }
    }
}
