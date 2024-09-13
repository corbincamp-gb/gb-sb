using SkillBridge.Business.Data;
using SkillBridge.Business.Model.Db;
using Taku.Core;

namespace SkillBridge.Business.Query
{
    public interface IOpportunityCollectionQuery :IQuery
    {
        IEnumerable<IOpportunity> Get();
    }

    public class OpportunityCollectionQuery : IOpportunityCollectionQuery
    {
        private readonly ApplicationDbContext _db;

        public OpportunityCollectionQuery(ApplicationDbContext db)
        {
            _db = db;
        }

        public IEnumerable<IOpportunity> Get()
        {
            return _db.Opportunities;
        }
    }
}
