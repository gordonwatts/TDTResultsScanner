using Jenkins.Domain;
using System;
using System.Linq;

namespace ScanTDTResults
{
    class Program
    {
        static void Main(string[] args)
        {
            Akavache.BlobCache.ApplicationName = "ScanTDTResults";

            var cl = new TDTJobAccess();
            var lastBuild = cl.GetLastBuild().Result;

            Console.WriteLine("Looking at build id={0}", lastBuild.Id);
            if (lastBuild.IsBuilding)
            {
                Console.WriteLine("  This build isn't finished!");
                return;
            }
            if (lastBuild.Status != JenkinsBuildStatus.Success)
            {
                Console.WriteLine("  Build didn't complete successfully!");
            }
            else
            {
                Console.WriteLine("  Build completed ok");
            }

            // Get all the runs in the build.
            var runs = cl.GetRunsForBuild(lastBuild).Result;

            // Now, look at one for each build, and analyze the log file.
            var byBuild = from r in runs
                          group r by r.RootCoreRelease;
            var buildList = from b in byBuild
                            select b.First();
            foreach (var b in buildList)
            {
                Console.WriteLine("Build report for release {0}", b.RootCoreRelease);
                b.BuildReport.WriteReport(Console.Out, "  ").Wait();
            }
        }
    }
}
