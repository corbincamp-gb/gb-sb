using SkillBridge.Business.Repository;
using Taku.Core;

namespace SkillBridge.Business.Query
{
    public interface IOrganizationCollectionQuery : IQuery 
    {
        IEnumerable<ILocation> Get();
    }

    public class OrganizationCollectionQuery : IOrganizationCollectionQuery
    {
        public IEnumerable<ILocation> Get()
        {
            return Enumerable.Empty<ILocation>();
        }

    }
}
