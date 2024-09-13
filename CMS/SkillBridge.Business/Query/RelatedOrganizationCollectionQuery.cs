using SkillBridge.Business.Data;
using SkillBridge.Business.Mapping;
using SkillBridge.Business.Model;
using Taku.Core;
using Z.EntityFramework.Plus;

namespace SkillBridge.Business.Query
{
    public interface IRelatedOrganizationCollectionQuery :IQuery
    {
        IRelatedOrganizationCollection Get();
    }

    public class RelatedOrganizationCollectionQuery : IRelatedOrganizationCollectionQuery
    {
        private readonly ApplicationDbContext _db;
        private readonly IRelatedOrganizationMapping _relatedOrganizationMapping;
        private readonly IRelatedOrganizationCollectionMapping _relatedOrganizationCollectionMapping;

        public RelatedOrganizationCollectionQuery(ApplicationDbContext db, 
            IRelatedOrganizationMapping relatedOrganizationMapping, 
            IRelatedOrganizationCollectionMapping relatedOrganizationCollectionMapping)
        {
            _db = db;
            _relatedOrganizationMapping = relatedOrganizationMapping;
            _relatedOrganizationCollectionMapping = relatedOrganizationCollectionMapping;
        }
        public IRelatedOrganizationCollection Get()
        {
            //// Get Unique Companies
            var uniqueParentOrgItems = _db.Opportunities.FromCache()
                                            .OrderBy(o => o.Organization_Name)
                                            .Select(m => m.Organization_Name)
                                            .Distinct().ToList();
         
            var orgs = _db.Organizations.ToList();
       
            // Find all Orgs under each parent org
            var orgData = (from parentOrg in uniqueParentOrgItems
                let uniqueOrgItems = orgs.Where(m => m.Parent_Organization_Name == parentOrg)
                    .OrderBy(m => m.Parent_Organization_Name)
                    .Distinct()
                    .Select(m => m.Name)
                    .ToList()
                select _relatedOrganizationMapping.Map(parentOrg, uniqueOrgItems)).ToList();
            return _relatedOrganizationCollectionMapping.Map(orgData);

        }
    }
}
