using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skillbridge.Business.Data;
using Taku.Core;

namespace Skillbridge.Business.Repository
{
    public interface ILocationRepository : IRepository
    {
    }

    public class OrganizationRepository : ILocationRepository
    {
        private readonly ApplicationDbContext _db;

        /// <summary>
        /// Gets all locations
        /// </summary>
        /// <returns>IEnumerable of ILocation</returns>
        public IEnumerable<ILocation> Fetch()
        {
            return Enumerable.Empty<ILocation>();
        }
    }

    public interface ILocation
    {
    }
}
