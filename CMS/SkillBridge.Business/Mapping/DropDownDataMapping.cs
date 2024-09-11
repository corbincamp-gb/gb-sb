using SkillBridge.Business.Model;
using SkillBridge.Business.Model.Db;
using Taku.Core;

namespace SkillBridge.Business.Mapping
{
    public interface IDropDownDataMapping : IMapping
    {
        IDropDownData Map(IEnumerable<IMilitaryBranch> milBranches, IEnumerable<IOpportunity> opportunities);
    }

    public class DropDownDataMapping : IDropDownDataMapping
    {
        public IDropDownData Map(IEnumerable<IMilitaryBranch> milBranches, IEnumerable<IOpportunity> opportunities)
        {
            Guard.AgainstNull(opportunities, "The opportunities list is null");
            Guard.Against(!opportunities.Any(), "The opportunities list has no data");

            return new DropDownDataModel()
            {
                MilitaryBranches = milBranches.Select(m => m.Name).ToArray(),
                Durations = opportunities.Select(d => d.Training_Duration).Distinct().ToArray(),
                Deliveries = opportunities.Select(o => o.Delivery_Method).Distinct().ToArray(),
                Locations = opportunities.Select(m => m.Locations_Of_Prospective_Jobs_By_State).Distinct().ToArray(),
                OccupationAreas = opportunities.Select(m => m.Job_Families).Distinct().ToArray(),
                Organizations = opportunities.Select(m => m.Organization_Name).Distinct().ToArray(),
                //RelatedOrganizations = o
            };
        }
    }
}
