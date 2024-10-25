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

    public class ProgramOrganizationCollectionQuery(ApplicationDbContext db) : IProgramOrganizationCollectionQuery
    {
        public IEnumerable<string> Get()
        {
            var progs = db.Programs.AsNoTracking();
            var opps = db.Opportunities.AsNoTracking();

            var uniquePrograms = progs.FromCache().Distinct().OrderBy(a => a.ProgramName).ToList();
            var orgIds = new HashSet<int>(uniquePrograms.Distinct().Select(up => up.OrganizationId).ToArray());

            var ret = new List<string>();

            var watch = Stopwatch.StartNew();

            foreach (var prog in uniquePrograms)
            {
                var oppList = new HashSet<IOpportunity>(opps.FromCache().Where(m => m.Program_Id == prog.Id && m.Is_Active).ToArray());
                //check to see how many programs in each org, if only one then dont output the org name with hyphen
                if (oppList.Count == 0 || !prog.IsActive) continue;
                var soloProgramUnderOrg = orgIds.Contains(prog.OrganizationId);
                var orgName = prog.OrganizationName.Trim();
                var progName = prog.ProgramName.Trim();
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
