using SkillBridge.Business.Data;
using SkillBridge.Business.Model.Db;
using Taku.Core;

namespace SkillBridge.Business.Repository
{
    public interface IOpportunityRepository : IRepository
    {
        IEnumerable<IOpportunity> Retrieve();
    }

    public class OpportunityRepository : IOpportunityRepository
    {
        private readonly ApplicationDbContext _db;

        public OpportunityRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public IEnumerable<IOpportunity> Retrieve()
        {
            var opps = _db.Opportunities;

            return opps;
        }
    }
}
