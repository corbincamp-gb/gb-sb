using SkillBridge.Business.Model;
using Taku.Core;

namespace SkillBridge.Business.Mapping
{
    public interface IRelateOrganizationMapping : IMapping
    {
        IRelatedOrganization Map(string parent, IEnumerable<string> orgs);
    }

    public class RelateOrganizationMapping : IRelateOrganizationMapping
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
