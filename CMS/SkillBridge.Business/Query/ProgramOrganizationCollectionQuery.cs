using System.Diagnostics;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SkillBridge.Business.Data;
using SkillBridge.Business.Model.Db;
using SkillBridge.Business.Repository;
using Taku.Core;
using Z.EntityFramework.Plus;

namespace SkillBridge.Business.Query
{
    public interface IProgramOrganizationCollectionQuery : IQuery
    {
        IEnumerable<string> Get();
    }

    public class ProgramOrganizationCollectionQuery : IProgramOrganizationCollectionQuery
    {
        private readonly ApplicationDbContext _db;
        public ProgramOrganizationCollectionQuery(ApplicationDbContext db)
        {
            _db = db;
        }

        public IEnumerable<string> Get()
        {
            var progs = _db.Programs.AsNoTracking();
            var opps = _db.Opportunities.AsNoTracking();


            //// Generate the string of JSON
            ////string newJson = "var locations = { data: [";
            //StringBuilder newJson = new StringBuilder("");

            //try
            //{
            //    var watch = Stopwatch.StartNew();
            //    /* PROGRAMS/PROVIDERS */
            //    // Get Unique Programs
            //    var uniquePrograms = progs.FromCache().OrderBy(a => a.Program_Name).ToList();
            //    // Sort Alphebetically
            //    //uniquePrograms.Sort();

            //    string uniqueProgramsForExport = "const programDropdown = new Array(";

            //    int numOutput = 0;

            //    for (var i = 0; i < uniquePrograms.Count; i++)
            //    {
            //        var progList = progs.FromCache().Where(m => m.Organization_Id == uniquePrograms[i].Organization_Id).ToList();
            //        var oppList = opps.FromCache().Where(m => m.Program_Id == uniquePrograms[i].Id).ToList();
            //        Debug.WriteLine("Program '" + uniquePrograms[i].Program_Name + "' has " + oppList.Count + " Opportunities attached to it");

            //        bool soloProgramUnderOrg = true;

            //        if (progList.Count > 1)
            //        {
            //            soloProgramUnderOrg = false;
            //        }

            //        bool hasActiveOpp = false;

            //        for (var j = 0; j < oppList.Count; j++)
            //        {
            //            if (oppList[j].Is_Active == true)
            //            {
            //                hasActiveOpp = true;
            //            }
            //        }

            //        //check to see how many programs in each org, if only one then dont output the org name with hyphen

            //        if (oppList.Count > 0 && hasActiveOpp == true && uniquePrograms[i].Is_Active == true)
            //        {
            //            var orgName = uniquePrograms[i].Organization_Name;//.Replace("'", @"\'");
            //            var progName = uniquePrograms[i].Program_Name;//.Replace("'", @"\'");

            //            if (numOutput == 0)
            //            {
            //                if (soloProgramUnderOrg == false || uniquePrograms[i].Program_Name != uniquePrograms[i].Organization_Name)
            //                {
            //                    uniqueProgramsForExport += "\"" + orgName + " - " + progName + "\"";
            //                }
            //                else
            //                {
            //                    uniqueProgramsForExport += "\"" + progName + "\"";
            //                }
            //            }
            //            else
            //            {
            //                if (soloProgramUnderOrg == false || uniquePrograms[i].Program_Name != uniquePrograms[i].Organization_Name)
            //                {
            //                    uniqueProgramsForExport += ", \"" + orgName + " - " + progName + "\"";
            //                }
            //                else
            //                {
            //                    uniqueProgramsForExport += ", \"" + progName + "\"";
            //                }
            //            }
            //            numOutput++;
            //        }
            //    }

            //    uniqueProgramsForExport += ");";

            //    Console.WriteLine("numOutput: " + numOutput);
            //    watch.Stop();
            //    Debug.WriteLine($"Total program time: {watch.Elapsed.Minutes}:{watch.Elapsed.Seconds}");
            //    // Add to main body of data to export
            //    return null;
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine("e: " + e.Message);
            //}

            //return new List<string>();
            var uniquePrograms = progs.FromCache().Distinct().OrderBy(a => a.Program_Name).ToList();
            var orgIds = new HashSet<int>(uniquePrograms.Distinct().Select(up => up.Organization_Id).ToArray());

            var ret = new List<string>();

            var watch = Stopwatch.StartNew();

            foreach (var prog in uniquePrograms)
            {
                var oppList = new HashSet<IOpportunity>(opps.FromCache().Where(m => m.Program_Id == prog.Id && m.Is_Active).ToArray());
                //check to see how many programs in each org, if only one then dont output the org name with hyphen
                if (oppList.Count == 0 || !prog.Is_Active) continue;
                var soloProgramUnderOrg = orgIds.Contains(prog.Organization_Id);
                var orgName = prog.Organization_Name.Trim();
                var progName = prog.Program_Name.Trim();
                Debug.WriteLine($"Adding {progName}");
                ret.Add(!soloProgramUnderOrg
                            || orgName != progName
                        ? $"{orgName} - {progName}"
                        : progName);
            }
            watch.Stop();
            Debug.Write($"Elapsed time: {watch.Elapsed.Minutes}:{watch.Elapsed.Seconds}");
            return ret.OrderBy(r => r).ToArray();
        }
    }
}
