using SkillBridge.Business.Model;
using Taku.Core;

namespace SkillBridge.Business.Mapping
{
    public interface IRelatedOrganizationMapping : IMapping
    {
        IRelatedOrganization Map(string parent, IEnumerable<string> orgs);
    }

    public class RelatedOrganizationMapping : IRelatedOrganizationMapping
    {
        public IRelatedOrganization Map(string parent, IEnumerable<string> orgs)
        {
            return new RelatedOrganizationModel
            {
                Organizations = orgs,
                Parent = parent
            };
        }
    }
}
