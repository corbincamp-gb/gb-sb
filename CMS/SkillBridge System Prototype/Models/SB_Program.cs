using CsvHelper.Configuration.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SkillBridge_System_Prototype.Models
{
    public class SB_Program
    {
        [Key]
        
        public int Id { get; set; }
        [Required]
        [Display(Name = "Program/Program Office/Agency")]
        [JsonProperty("PROGRAM")]
        public string Program_Name { get; set; }
        public string Organization_Name { get; set; }
        public int Organization_Id { get; set; }
        [JsonIgnore]
        [ForeignKey("Organization_Id")]
        public virtual SB_Organization SB_Organization { get; set; }
        [Required]
        [Display(Name = "Is Active")]
        [JsonIgnore]
        public bool Is_Active { get; set; }
        [Display(Name = "Date Deactivated")]
        [JsonIgnore]
        public DateTime Date_Deactivated { get; set; }
        [Display(Name = "LHN Intake Ticket Id")]
        public string Lhn_Intake_Ticket_Id { get; set; } = "";  // LHN Intake Ticket Number
        [Display(Name = "Has Intake")]
        public bool Has_Intake { get; set; }        // Do we have a completed QuestionPro intake form from them
        [Display(Name = "Intake Form Version")]
        public string Intake_Form_Version { get; set; } = "";  // Which version of the QuestionPro intake form did they fill out
        [Display(Name = "QP Intake Submission Id")]
        public string Qp_Intake_Submission_Id { get; set; } = ""; // The ID of the QuestionPro intake form submission
        [Display(Name = "Location Details Available")]
        [JsonProperty("LOCATION_DETAILS_AVAILABLE")]
        //public bool Has_Locations { get; set; }      // From col N of master spreadsheet
        public bool Location_Details_Available { get; set; } // From col O of master spreadsheet
        [Display(Name = "Has Consent")]
        public bool Has_Consent { get; set; }
        [Display(Name = "Qp Location Submission Id")]
        public string Qp_Location_Submission_Id { get; set; } = "";
        [Display(Name = "LHN Location Ticket Id")]
        public string Lhn_Location_Ticket_Id { get; set; } = "";
        [Display(Name = "Has Multiple Locations")]
        public bool Has_Multiple_Locations { get; set; }
        [Display(Name = "Reporting Form 2020")]
        public bool Reporting_Form_2020 { get; set; }
        [Required]
        [Display(Name = "Date Authorized")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime Date_Authorized { get; set; }  // Date the 
        [Required]
        [Display(Name = "MOU Link")]
        [DataType(DataType.Url)]
        [RegularExpression(@"(https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|www\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9]+\.[^\s]{2,}|www\.[a-zA-Z0-9]+\.[^\s]{2,})", ErrorMessage = "Please provide a valid URL, you may need to include http:// or https://")]
        public string Mou_Link { get; set; } = "";       // URL link to actual MOU packet
        [Display(Name = "MOU Creation Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime Mou_Creation_Date { get; set; }
        [Display(Name = "MOU Expiration Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime Mou_Expiration_Date { get; set; }
        [JsonProperty("NATIONWIDE")]
        public bool Nationwide { get; set; }
        [JsonProperty("ONLINE")]
        public bool Online { get; set; }
        [Display(Name = "Participation Populations")]
        public string Participation_Populations { get; set; }  // Might want enum for this
        [Display(Name = "Delivery Method")]
        [JsonProperty("DELIVERY_METHOD")]
        public string Delivery_Method { get; set; }
        [Display(Name = "States Of Program Delivery")]
        [JsonProperty("STATES")]
        public string States_Of_Program_Delivery { get; set; } = "";
        [Required]
        [Display(Name = "Program Duration")]
        [JsonProperty("PROGRAM_DURATION")]
        public int Program_Duration { get; set; }
        [Display(Name = "Support Cohorts")]
        [JsonProperty("COHORTS")]
        public bool Support_Cohorts { get; set; }
        [Required]
        [Display(Name = "Opportunity Type")]
        [JsonProperty("OPPORTUNITY_TYPE")]
        public string Opportunity_Type { get; set; }
        [Display(Name = "Job Family")]
        [JsonProperty("JOB_FAMILY")]
        public string Job_Family { get; set; } = "";
        [Display(Name = "Services Supported")]
        public string Services_Supported { get; set; } = "";
        [Display(Name = "Enrollment Dates")]
        public string Enrollment_Dates { get; set; } = "";
        [Display(Name = "Date Created")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime Date_Created { get; set; } // Date program was created in system
        [Display(Name = "Date Updated")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime Date_Updated { get; set; } // Date program was last edited/updated in the system
        [Display(Name = "Created By")]
        public string Created_By { get; set; }
        [Display(Name = "Updated By")]
        public string Updated_By { get; set; }
        [Display(Name = "Program URL")]
        [DataType(DataType.Url)]
        [RegularExpression(@"(https?:\/\/(?:www[a-zA-Z0-9]\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|www\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9]+\.[^\s]{2,}|www\.[a-zA-Z0-9]+\.[^\s]{2,}|https?:\/\/www\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,})", ErrorMessage = "Please provide a valid URL, you may need to include http:// or https://")]
        public string Program_Url { get; set; } = "";
        [Display(Name = "Program Status")]
        public bool Program_Status { get; set; } // false is disabled, true is enabled
        [Required]
        [Display(Name = "Admin POC First Name")]
        public string Admin_Poc_First_Name { get; set; }
        [Required]
        [Display(Name = "Admin POC Last Name")]
        public string Admin_Poc_Last_Name { get; set; }
        [Required]
        [Display(Name = "Admin POC Email")]
		[RegularExpression(@"^(.+)@(.{2,})\.(.{2,})$", ErrorMessage = "Please provide a valid email address")]
		public string Admin_Poc_Email { get; set; }
        [Required]
        [Display(Name = "Admin POC Phone Number")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid phone number")]
        public string Admin_Poc_Phone { get; set; }
        [Required]
        [Display(Name = "Public POC Name")]
        public string Public_Poc_Name { get; set; }
        [Required]
        [Display(Name = "Public POC Email")]
		[RegularExpression(@"^(.+)@(.{2,})\.(.{2,})$", ErrorMessage = "Please provide a valid email address")]
		public string Public_Poc_Email { get; set; }
        public string Notes { get; set; } = "";
        [Display(Name = "For Spouses")]
        public bool For_Spouses { get; set; }
        public int Legacy_Program_Id { get; set; }
        public int Legacy_Provider_Id { get; set; }

        [Display(Name = "Job Family")]
        [JsonIgnore]
        public virtual List<ProgramJobFamily> ProgramJobFamilies { get; set; }

        [Display(Name = "Participation Populations")]
        [JsonIgnore]
        public virtual List<ProgramParticipationPopulation> ProgramParticipationPopulations { get; set; }

        [Display(Name = "Services Supported")]
        [JsonIgnore]
        public virtual List<ProgramService> ProgramServices { get; set; }

        [Display(Name = "States of Program Delivery")]
        [JsonIgnore]
        public virtual List<ProgramState> ProgramStates { get; set; }

        [Display(Name = "Delivery Method")]
        [JsonIgnore]
        public virtual List<ProgramDeliveryMethod> ProgramDeliveryMethods { get; set; }

        [JsonIgnore]
        public virtual List<ProgramTrainingPlan> ProgramTrainingPlans { get; set; }

    }
}
