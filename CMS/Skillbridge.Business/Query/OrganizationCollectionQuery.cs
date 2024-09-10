using Skillbridge.Business.Repository;
using Taku.Core;

namespace Skillbridge.Business.Query
{
    public class OrganizationCollectionQuery : IQuery
    {
        public IEnumerable<ILocation> Get()
        {
            return Enumerable.Empty<ILocation>();
        }

    }
}
