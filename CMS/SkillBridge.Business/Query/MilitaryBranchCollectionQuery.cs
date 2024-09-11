using SkillBridge.Business.Data;
using SkillBridge.Business.Model.Db;
using SkillBridge.Business.Repository;
using Taku.Core;

namespace SkillBridge.Business.Query
{
    public interface IMilitaryBranchCollectionQuery :IQuery
    {
        IEnumerable<IMilitaryBranch> Get();
    }

    public class MilitaryBranchCollectionQuery : IMilitaryBranchCollectionQuery
    {
        private readonly IMilitaryBranchRepository _repo;

        public MilitaryBranchCollectionQuery(IMilitaryBranchRepository repo)
        {
            _repo = repo;
        }

        public IEnumerable<IMilitaryBranch> Get()
        {
            return _repo.Retrieve();
        }
    }
}
