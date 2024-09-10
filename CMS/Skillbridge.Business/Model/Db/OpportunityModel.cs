using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Skillbridge.Business.Util;

namespace Skillbridge.Business.Model.Db
{
    public class OpportunityModel
    {
        [Key]
        [JsonProperty("ID")]
        public int Id { get; set; }
        //[Required]
        [Display(Name = "Group ID")]
        [JsonProperty("GROUPID")]
        public int Group_Id { get; set; }
            
        [ForeignKey("SB_Organization")]
        [JsonIgnore]
        public int Organization_Id { get; set; }
        //public SB_Organization SB_Organization { get; set; }
        [JsonIgnore]
        public int Program_Id { get; set; }
        [Required]
        [Display(Name = "Is Active")]
        [JsonProperty("ISACTIVE")]
        [JsonIgnore]
        public bool Is_Active { get; set; }
        [ForeignKey("Program_Id")]
        public ProgramModel SB_Program { get; set; }
        [Display(Name = "Date Deactivated")]
        [JsonIgnore]
        public DateTime Date_Deactivated { get; set; }
        [Required]
        [Display(Name = "Program/Program Office/Agency")]
        [JsonProperty("PROGRAM")]
        public string Program_Name { get; set; }
        [Display(Name = "Opportunity URL")]
        [DataType(DataType.Url)]
        [RegularExpression(@"(https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|www\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9]+\.[^\s]{2,}|www\.[a-zA-Z0-9]+\.[^\s]{2,})", ErrorMessage = "Please provide a valid URL, you may need to include http:// or https://")]
        //[Newtonsoft.Json.JsonProperty("URL")]
        [JsonIgnore]
        public string Opportunity_Url { get; set; }
        [Required]
        [Display(Name = "Date Program Inititated")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        [JsonProperty("DATEPROGRAMINITIATED")]
        [JsonConverter(typeof(OpportunityDateTimeConverter))]
        public DateTime Date_Program_Initiated { get; set; }
        [Display(Name = "Date Created")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        [JsonIgnore]
        public DateTime Date_Created { get; set; } // Date opportunity was created in system
        [Display(Name = "Date Updated")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        [JsonIgnore]
        public DateTime Date_Updated { get; set; } // Date opportunity was last edited/updated in the system
        [Required]
        [Display(Name = "Employer POC Name")]
        [JsonProperty("EMPLOYERPOC")]
        public string Employer_Poc_Name { get; set; }
        [Required]
        [Display(Name = "Employer POC Email")]
        [JsonProperty("EMPLOYERPOCEMAIL")]
		[RegularExpression(@"^(.+)@(.{2,})\.(.{2,})$", ErrorMessage = "Please provide a valid email address")]
		public string Employer_Poc_Email { get; set; }
        [Required]
        [Display(Name = "Training Duration")]
        [JsonProperty("DURATIONOFTRAINING")]
        public string Training_Duration { get; set; }
        [Required]
        [Display(Name = "Service")]
        [JsonProperty("SERVICE")]
        public string Service { get; set; }
        [Required]
        [Display(Name = "Delivery Method")]
        [JsonProperty("DELIVERY_METHOD")]
        public string Delivery_Method { get; set; }
        [Display(Name = "Multiple Locations")]
        [JsonIgnore]
        public bool Multiple_Locations { get; set; }
        [Required]
        [Display(Name = "Program Type")]
        [JsonIgnore]
        public string Program_Type { get; set; }
        [Display(Name = "Job Families")]
        [JsonIgnore]
        public string Job_Families { get; set; }
        [Display(Name = "Participation Populations")]
        [JsonIgnore]
        public string Participation_Populations { get; set; }
        [Display(Name = "Support Cohorts")]
        [JsonIgnore]
        public bool Support_Cohorts { get; set; }
        [Display(Name = "Enrollment Dates")]
        [JsonIgnore]
        public string Enrollment_Dates { get; set; }
        [Display(Name = "MOUs")]
        [JsonProperty("MOUs")]
        public bool Mous { get; set; }
        [Display(Name = "Number of Locations")]
        [JsonIgnore]
        public int Num_Locations { get; set; }
        [Display(Name = "Installation")]
        [JsonProperty("INSTALLATION")]
        public string Installation { get; set; }
        [Display(Name = "City")]
        [JsonProperty("CITY")]
        public string City { get; set; }
        [Display(Name = "State")]
        [JsonProperty("STATE")]
        public string State { get; set; }
        [Display(Name = "Zip")]
        [JsonProperty("ZIP")]
        public string Zip { get; set; }
        [Display(Name = "Latitude")]
        [JsonProperty("LAT")]
        public double Lat { get; set; }
        [Display(Name = "Longitude")]
        [JsonProperty("LONG")]
        public double Long { get; set; }
        [JsonProperty("NATIONWIDE")]
        public bool Nationwide { get; set; }
        [JsonIgnore]
        public bool Online { get; set; }
        [Required]
        [Display(Name = "Summary Description")]
        [JsonProperty("SUMMARYDESCRIPTION")]
        public string Summary_Description { get; set; }
        [Required]
        [Display(Name = "Jobs Description")]
        [JsonProperty("JOBSDESCRIPTION")]
        public string Jobs_Description { get; set; }
        [Display(Name = "Links to Prospective Jobs")]
        [JsonIgnore]
        public string Links_To_Prospective_Jobs { get; set; }
        [Display(Name = "Locations of Prospective Jobs by State")]
        [JsonProperty("LOCATIONSOFPROSPECTIVEJOBSBYSTATE")]
        public string Locations_Of_Prospective_Jobs_By_State { get; set; }
        [Display(Name = "Salary")]
        [JsonProperty("SALARY")]
        public string Salary { get; set; }
        [Display(Name = "Prospective Job Labor Demand")]
        [JsonIgnore]
        public string Prospective_Job_Labor_Demand { get; set; }
        [Display(Name = "Target MOCs")]
        [JsonProperty("TARGETMOCs")]
        public string Target_Mocs { get; set; }
        [Display(Name = "Other Eligibility Factors")]
        [JsonProperty("OTHERELIGIBILITYFACTORS")]
        public string Other_Eligibility_Factors { get; set; }
        [Display(Name = "Cost")]
        [Required]
        [JsonProperty("COST")]
        public string Cost { get; set; }
        [Display(Name = "Other")]
        [Required]
        [JsonProperty("OTHER")]
        public string Other { get; set; }
        [JsonIgnore]
        public string Notes { get; set; }
        [Display(Name = "Created By")]
        [JsonIgnore]
        public string Created_By { get; set; }
        [Display(Name = "Updated By")]
        [JsonIgnore]
        public string Updated_By { get; set; }
        [Display(Name = "For Spouses")]
        [JsonIgnore]
        public bool For_Spouses { get; set; }

        // For Opp List View optimization
        [Display(Name = "Organization Name")]
        [JsonIgnore]
        public string Organization_Name { get; set; }   // this is here so loading the list
        [Display(Name = "MOU Link")]
        [JsonIgnore]
        [RegularExpression(@"(https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|www\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9]+\.[^\s]{2,}|www\.[a-zA-Z0-9]+\.[^\s]{2,})", ErrorMessage = "Please provide a valid URL, you may need to include http:// or https://")]
        public string Mou_Link { get; set; }       // URL link to actual MOU packet
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        [Display(Name = "MOU Expiration Date")]
        [JsonIgnore]
        public DateTime Mou_Expiration_Date { get; set; }
        [Display(Name = "Admin POC First Name")]
        [JsonIgnore]
        public string Admin_Poc_First_Name { get; set; }
        [Display(Name = "Admin POC Last Name")]
        [JsonIgnore]
        public string Admin_Poc_Last_Name { get; set; }
        [Display(Name = "Admin POC Email")]
        [JsonIgnore]
		[RegularExpression(@"^(.+)@(.{2,})\.(.{2,})$", ErrorMessage = "Please provide a valid email address")]
		public string Admin_Poc_Email { get; set; }
        [JsonIgnore]

        public int Legacy_Opportunity_Id { get; set; }
        [JsonIgnore]
        public int Legacy_Program_Id { get; set; }
        [JsonIgnore]
        public int Legacy_Provider_Id { get; set; }

        public string GetLocationDisplay()
        {
            var html = new System.Text.StringBuilder();

            if (!string.IsNullOrWhiteSpace(Installation))
            {
                html.Append($"{Installation}, ");
            }

            html.Append($"{City}, {State} {Zip}");

            return html.ToString();
        }
    }
}
