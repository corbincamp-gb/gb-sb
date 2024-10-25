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

    public class RelatedOrganizationCollectionQuery(
        ApplicationDbContext db,
        IRelatedOrganizationMapping relatedOrganizationMapping,
        IRelatedOrganizationCollectionMapping relatedOrganizationCollectionMapping)
        : IRelatedOrganizationCollectionQuery
    {
        public IRelatedOrganizationCollection Get()
        {
            //// Get Unique Companies
            var uniqueParentOrgItems = db.Opportunities.FromCache()
                                            .OrderBy(o => o.Organization_Name)
                                            .Select(m => m.Organization_Name)
                                            .Distinct().ToList();
         
            var orgs = db.Organizations.ToList();
       
            // Find all Orgs under each parent org
            var orgData = (from parentOrg in uniqueParentOrgItems
                let uniqueOrgItems = orgs.Where(m => m.Parent_Organization_Name == parentOrg)
                    .OrderBy(m => m.Parent_Organization_Name)
                    .Distinct()
                    .Select(m => m.Name)
                    .ToList()
                select relatedOrganizationMapping.Map(parentOrg, uniqueOrgItems)).ToList();
            return relatedOrganizationCollectionMapping.Map(orgData);

        }
    }
}
