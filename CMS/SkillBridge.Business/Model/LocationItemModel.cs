using System.Text.Json.Serialization;

namespace SkillBridge.Business.Model
{
    public interface ILocationItem
    {
        [JsonPropertyName("COST")]
        string Cost { get; set; }
        [JsonPropertyName("SERVICE")]
        string Service { get; set; }
        [JsonPropertyName("PARENTORGANIZATION")]
        string ParentOrganization { get; set; }
        [JsonPropertyName("ORGANIZATION")]
        string Organization { get; set; }
        [JsonPropertyName("PROGRAM")]
        string Program { get; set; }
        [JsonPropertyName("INSTALLATION")]
        string Installation { get; set; }
        [JsonPropertyName("CITY")]
        string City { get; set; }
        [JsonPropertyName("STATE")]
        string State { get; set; }
        [JsonPropertyName("ZIP")]
        string Zip { get; set; }
        [JsonPropertyName("EMPLOYERPOC")]
        string EmployerPOC { get; set; }
        [JsonPropertyName("EMPLOYERPOCEMAIL")]
        string EmployerPOCEmail { get; set; }
        [JsonPropertyName("DURATIONOFTRAINING")]
        string DurationOfTraining { get; set; }
        [JsonPropertyName("SUMMARYDESCRIPTION")]
        string SummaryDescription { get; set; }
        [JsonPropertyName("JOBDESCRIPTION")]
        string JobDescription { get; set; }
        [JsonPropertyName("LOCATIONSOFPROSPECTIVEJOBSBYSTATE")]
        string LocationsOfProspecitveJobsByState { get; set; }
        [JsonPropertyName("TARGETMOCS")]
        string TargetMOCs { get; set; }
        [JsonPropertyName("OTHERELIGITIBLITYFACTORS")]
        string OtherEligibilityFactors { get; set; }
        [JsonPropertyName("OTHER")]
        string Other { get; set; }
        [JsonPropertyName("MOUS")]
        string MOUs { get; set; }
        [JsonPropertyName("LAT")]
        double Lat { get; set; }
        [JsonPropertyName("LONG")]
        double Long { get; set; }
        [JsonPropertyName("NATIONWIDE")]
        int NationWide { get; set; }
        [JsonPropertyName("DELIVERYMETHOD")]
        string DeliveryMethod { get; set; }
        [JsonPropertyName("JOBFAMILES")]
        string JobFamilies { get; set; }
    }

    public class LocationItemModel : ILocationItem
    {
        public string Cost { get; set; }

        public string Service { get; set; }
        public string ParentOrganization { get; set; }
        public string Organization { get; set; }
        public string Program { get; set; }
        public string Installation { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string EmployerPOC { get; set; }
        public string EmployerPOCEmail { get; set; }
        public string DurationOfTraining { get; set; }
        public string SummaryDescription { get; set; }
        public string JobDescription { get; set; }
        public string LocationsOfProspecitveJobsByState { get; set; }
        public string TargetMOCs { get; set; }
        public string OtherEligibilityFactors { get; set; }
        public string Other { get; set; }
        public string MOUs { get; set; }
        public double Lat { get; set; }
        public double Long { get; set; }
        public int NationWide { get; set; }
        public string DeliveryMethod { get; set; }
        public string JobFamilies { get; set; } 
    }

    
}
