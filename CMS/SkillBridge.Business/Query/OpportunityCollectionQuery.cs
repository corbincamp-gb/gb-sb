using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkillBridge.Business.Model.Db;
using SkillBridge.Business.Repository;
using Taku.Core;

namespace SkillBridge.Business.Query
{
    public interface IOpportunityCollectionQuery :IQuery
    {
        IEnumerable<IOpportunity> Get();
    }

    public class OpportunityCollectionQuery : IOpportunityCollectionQuery
    {
        private readonly IOpportunityRepository _repo;

        public OpportunityCollectionQuery(IOpportunityRepository repo)
        {
            _repo = repo;
        }

        public IEnumerable<IOpportunity> Get()
        {
            return _repo.Retrieve();
        }
    }
}
