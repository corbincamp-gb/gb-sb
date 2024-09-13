using System.ComponentModel.DataAnnotations;

namespace Taku.Core.Enums
{
    public enum JobFamily
    {
        [Display(Name = "Architecture and Engineering")] ArchitectureAndEngineering,

        [Display(Name = "Arts, Design, Entertainment, Sports, and Media")] ArtsDesignEntertainmentSportsAndMedia,

        [Display(Name = "Building and Grounds Cleaning and Maintenance")] BuildingAndGroundsCleaningAndMaintenance,

        [Display(Name = "Business and Financial Operations")] BusinessAndFinancialOperations,

        [Display(Name = "Community and Social Service")] CommunityAndSocialService,

        [Display(Name = "Computer and Mathematical")] ComputerAndMathematical,

        [Display(Name = "Construction and Extraction")] ConstructionAndExtraction,

        [Display(Name = "Education, Training, and Library")] EducationTrainingAndLibrary,

        [Display(Name = "Farming, Fishing, and Forestry")] FarmingFishingAndForestry,

        [Display(Name = "Food Preparation and Serving Related")] FoodPreparationAndServingRelated,

        [Display(Name = "Healthcare Practitioners and Technical")] HealthcarePractitionersAndTechnical,

        [Display(Name = "Healthcare Support")]HealthcareSupport,

        [Display(Name = "Installation, Maintenance, and Repair")] InstallationMaintenanceAndRepair,

        [Display(Name = "Legal")] Legal,

        [Display(Name = "Life, Physical, and Social Science")] Life, PhysicalAndSocialScience,

        [Display(Name = "Management")] Management,

        [Display(Name = "Military Specific")] MilitarySpecific,

        [Display(Name = "Office and Administrative Support")] OfficeAndAdministrativeSupport,

        [Display(Name = "Personal Care and Service")] PersonalCareAndService,

        [Display(Name = "Production")] Production,

        [Display(Name = "Protective Service")] ProtectiveService,

        [Display(Name = "Sales and Related")] SalesAndRelated,

        [Display(Name = "Transportation and Material Moving")] TransportationAndMaterialMoving,

        [Display(Name = "Other")] Other

    }
}
