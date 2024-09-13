using SkillBridge.Business.Model;
using Taku.Core;

namespace SkillBridge.Business.Mapping
{
    public interface IRelatedOrganizationCollectionMapping : IMapping
    {
        IRelatedOrganizationCollection Map(IEnumerable<IRelatedOrganization> relatedOrganizations = null);
    }

    public class RelatedOrganizationCollectionMapping : IRelatedOrganizationCollectionMapping
    {
        public IRelatedOrganizationCollection Map(IEnumerable<IRelatedOrganization> relatedOrganizations = null)
        {
            return new RelatedOrganizationCollectionModel
            {
                Data = relatedOrganizations ?? Enumerable.Empty<IRelatedOrganization>()
            };
        }
    }
}
