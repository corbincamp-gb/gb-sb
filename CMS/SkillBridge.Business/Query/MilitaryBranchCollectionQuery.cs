using Microsoft.EntityFrameworkCore;
using SkillBridge.Business.Data;
using SkillBridge.Business.Model.Db;
using Taku.Core;

namespace SkillBridge.Business.Query
{
    public interface IMilitaryBranchCollectionQuery :IQuery
    {
        IEnumerable<IMilitaryBranch> Get();
    }

    public class MilitaryBranchCollectionQuery : IMilitaryBranchCollectionQuery
    {
        private readonly ApplicationDbContext _db;

        public MilitaryBranchCollectionQuery(ApplicationDbContext db)
        {
            _db = db;
        }

        public IEnumerable<IMilitaryBranch> Get()
        {
            return _db.MilitaryBranches.AsNoTracking();
        }
    }
}
