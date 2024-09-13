using SkillBridge.Business.Repository;
using Taku.Core;

namespace SkillBridge.Business.Query
{
    public class OrganizationCollectionQuery : IQuery
    {
        public IEnumerable<ILocation> Get()
        {
            return Enumerable.Empty<ILocation>();
        }

    }
}
