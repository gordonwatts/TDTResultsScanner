using Jenkins.Domain;
using System;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace ScanTDTResults
{
    static class JenkinsUtils
    {
        /// <summary>
        /// Download a file to a temp location.
        /// </summary>
        /// <param name="artifact"></param>
        /// <param name="extension">The extension the file shoudl have, without the dot.</param>
        /// <returns></returns>
        public static Task<FileInfo> SaveToTempFile(this JenkinsArtifact artifact, JenkinsBuild bld, string extension)
        {
            var fname = string.Format("{0}{1}.{2}", Path.GetTempPath(), Guid.NewGuid(), extension);
            return artifact.SaveToFile(bld, new FileInfo(fname));
        }

        /// <summary>
        /// Save to a file.
        /// </summary>
        /// <param name="?"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public static async Task<FileInfo> SaveToFile(this JenkinsArtifact artifact, JenkinsBuild bld, FileInfo destination)
        {
            var url = string.Format("{0}/{1}", bld.Url, artifact.RelativePath);
            var fileData = await TDTJobAccess.GetAsString(url).FirstAsync();
            using (var wr = destination.Create())
            {
                using (var txtwr = new StreamWriter(wr))
                {
                    txtwr.Write(fileData);
                }
                wr.Close();
            }
            return destination;
        }
    }
}
