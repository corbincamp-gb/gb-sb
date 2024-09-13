using SkillBridge.Business.Model;
using SkillBridge.Business.Model.Db;
using SkillBridge.Business.Query;
using Taku.Core;

namespace SkillBridge.Business.Mapping
{
    public interface IDropDownDataMapping : IMapping
    {
        IDropDownData Map(IEnumerable<IMilitaryBranch> milBranches, IEnumerable<IOpportunity> opportunities, IEnumerable<string> programOrgs, IRelatedOrganizationCollection relatedOrganizationCollection);
    }

    public class DropDownDataMapping : IDropDownDataMapping
    {
        private readonly IDeliveryMethodQuery _deliveryMethodQuery;

        public DropDownDataMapping(IDeliveryMethodQuery deliveryMethodQuery)
        {
            _deliveryMethodQuery = deliveryMethodQuery;
        }

        public IDropDownData Map(IEnumerable<IMilitaryBranch> milBranches, IEnumerable<IOpportunity> opportunities, IEnumerable<string> programOrgs, IRelatedOrganizationCollection relatedOrganizationCollection)
        {
            Guard.AgainstNull(opportunities, "The opportunities list is null");
            Guard.Against(!opportunities.Any(), "The opportunities list has no data");

            return new DropDownDataModel()
            {
                Programs = programOrgs,
                MilitaryBranches = milBranches.Select(m => m.Name).ToArray(),
                Durations = opportunities.Select(d => d.Training_Duration).Where(d => d.Length > 0).Distinct().ToArray(),
                Deliveries = opportunities.Select(o => _deliveryMethodQuery.Get(int.Parse(o.Delivery_Method == string.Empty ? "0" : o.Delivery_Method) + 1)).Distinct().ToArray(),
                Locations = opportunities.Where(o => o.Locations_Of_Prospective_Jobs_By_State.Trim().Length > 0).Select(m => m.Locations_Of_Prospective_Jobs_By_State.Trim()).Distinct().ToArray(),
                OccupationAreas = opportunities.Select(m => m.Job_Families).Distinct().ToArray(),
                Organizations = opportunities.Select(m => m.Organization_Name).Distinct().ToArray(),
                RelatedOrganizations = relatedOrganizationCollection
            };
        }
    }
}
