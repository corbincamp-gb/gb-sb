using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkillBridge.Business.Data;
using SkillBridge.Business.Model.Db;
using Taku.Core;

namespace SkillBridge.Business.Repository
{
    public interface IMilitaryBranchRepository : IRepository
    {
        IEnumerable<IMilitaryBranch> Retrieve();
    }

    public class MilitaryBranchRepository : IMilitaryBranchRepository
    {
        private readonly ApplicationDbContext _db;

        public MilitaryBranchRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public IEnumerable<IMilitaryBranch> Retrieve()
        {
            var branches = _db.MilitaryBranches;

            return branches;
        }
    }
}
