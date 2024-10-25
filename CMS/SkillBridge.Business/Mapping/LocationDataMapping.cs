using SkillBridge.Business.Model;
using SkillBridge.Business.Model.Db;
using Taku.Core;

namespace SkillBridge.Business.Mapping
{
    public interface ILocationDataMapping : IMapping
    {
        ILocationData Map(IEnumerable<IOrganization> orgs, IEnumerable<IOpportunity> opps, IEnumerable<IProgram> progs);
    }

    public class LocationDataMapping(ILocationItemMapping _locationItemMapping) : ILocationDataMapping
    {
        public ILocationData Map(IEnumerable<IOrganization> orgs, IEnumerable<IOpportunity> opps, IEnumerable<IProgram> progs)
        {
            var locs = (from opp in opps 
                                    let prog = progs.SingleOrDefault(x => x.Id == opp.Program_Id) 
                                    let org = orgs.SingleOrDefault(x => x.Id == opp.Organization_Id) 
                                    select _locationItemMapping.Mapping(opp, org, prog)).ToList();

            return new LocationDataModel
            {
                Locations = locs
            };
        }
    }

}
