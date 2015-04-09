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

            //
            // Get the basic job info, and make sure the last job is something we are ready to
            // go with.
            //

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

            //
            // Get the various runs, and analyze them.
            // First, look at them by-build. And then the results of each one.
            //

            Console.WriteLine();
            var runs = cl.GetRunsForBuild(lastBuild).Result;
            var byBuild = from r in runs
                          group r by r.RootCoreRelease;
            var buildList = from b in byBuild
                            select b.First();
            foreach (var b in buildList)
            {
                Console.WriteLine("Build report for release {0}", b.RootCoreRelease);
                b.BuildReport.WriteReportBuild(Console.Out, "  ").Wait();
            }

            Console.WriteLine();
            foreach (var b in runs)
            {
                Console.WriteLine("Build report for: r{1} - {0}", b.RootCoreRelease, b.AmiRecoReleaseTag);
                b.BuildReport.WriteReportRun(Console.Out, "  ").Wait();
            }
        }
    }
}
