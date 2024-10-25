using Microsoft.EntityFrameworkCore;
using SkillBridge.Business.Data;
using SkillBridge.Business.Mapping;
using SkillBridge.Business.Model;
using Taku.Core;

namespace SkillBridge.Business.Query
{
    public interface ILocationDataQuery : IQuery
    {
        ILocationData Get();
    }

    public class LocationDataQuery(ApplicationDbContext _db,
        ILocationDataMapping _locationDataMapping)
        : ILocationDataQuery
    {
        public ILocationData Get()
        {
            var orgs = _db.Organizations.AsNoTracking().Where(o => o.Is_Active).ToHashSet(); 
            var progs = _db.Programs.AsNoTracking().Where(p => p.IsActive).ToHashSet();
            var opps = _db.Opportunities.AsNoTracking().Where(o => o.Is_Active).ToHashSet();

            return _locationDataMapping.Map(orgs, opps, progs);
        }
    }
}
