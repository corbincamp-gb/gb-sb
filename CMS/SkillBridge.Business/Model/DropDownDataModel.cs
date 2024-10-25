﻿#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace SkillBridge.Business.Model
{
    public interface IDropDownData
    {
        IEnumerable<string> MilitaryBranches { get; set; }
        IEnumerable<string> Durations { get; set; }
        IEnumerable<string> Deliveries { get; set; }
        IEnumerable<string> Locations { get; set; }
        IEnumerable<string> OccupationAreas { get; set; }
        IEnumerable<string> Organizations { get; set; }
        IEnumerable<string> Programs { get; set; }
        IRelatedOrganizationCollection RelatedOrganizations { get; set; }
    }

    public class DropDownDataModel : IDropDownData
    {
        public IEnumerable<string> Programs { get; set; }
        public IEnumerable<string> MilitaryBranches { get; set; }
        public IEnumerable<string> Durations { get; set; }
        public IEnumerable<string> Deliveries { get; set; }
        public IEnumerable<string> Locations { get; set; }

        // family
        public IEnumerable<string> OccupationAreas { get; set;}

        // parentOrgs
        public IEnumerable<string> Organizations { get; set; }

        // relatedOrgs
        public IRelatedOrganizationCollection RelatedOrganizations { get; set; }



    }
}
