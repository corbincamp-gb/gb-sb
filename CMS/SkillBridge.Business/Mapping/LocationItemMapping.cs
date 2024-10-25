using SkillBridge.Business.Command;
using SkillBridge.Business.Model;
using SkillBridge.Business.Model.Db;
using SkillBridge.Business.Query;
using Taku.Core;

namespace SkillBridge.Business.Mapping
{
    public interface ILocationItemMapping : IMapping
    {
        ILocationItem Mapping(IOpportunity opp, IOrganization org, IProgram prog);
    }

    public class LocationItemMapping(IDeliveryMethodQuery _deliveryMethodQuery, IDetermineProgramDurationCommand _determineProgramDurationCommand) : ILocationItemMapping
    {
        public ILocationItem Mapping(IOpportunity opp, IOrganization org, IProgram prog)
        {
            if (opp == null || org == null)
            {
                return new LocationItemModel();
            }

            var programName = prog?.ProgramName ?? string.Empty;
            var newName = org.Name.Equals(programName, StringComparison.CurrentCultureIgnoreCase)
                ? org.Name
                : $"{org.Name} - {programName}";
                
            _determineProgramDurationCommand.Execute(prog, out var duration);

            return new LocationItemModel
            {
                Cost = opp.Cost,
                City = opp.City,
                DeliveryMethod = _deliveryMethodQuery.Get(opp.Delivery_Method),
                Program = newName,
                Installation = opp.Installation,
                State = opp.State,
                Zip = opp.Zip,
                EmployerPOC = opp.Employer_Poc_Name,
                EmployerPOCEmail = opp.Employer_Poc_Email,
                DurationOfTraining = duration,
                SummaryDescription = opp.Summary_Description,
                LocationsOfProspecitveJobsByState = opp.Locations_Of_Prospective_Jobs_By_State,
                TargetMOCs = opp.Target_Mocs,
                OtherEligibilityFactors = opp.Other_Eligibility_Factors,
                Other = opp.Other,
                MOUs = opp.Mous ? "Y":"N",
                Lat = opp.Lat,
                Long = opp.Long,
                NationWide = opp.Nationwide.GetHashCode(),
                JobDescription = opp.Jobs_Description,
                JobFamilies = opp.Job_Families,
                Service = opp.Service,
                ParentOrganization = org.Parent_Organization_Name,
                Organization = org.Name
            };
        }
    }

}
