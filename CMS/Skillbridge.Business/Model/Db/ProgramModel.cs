using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;


namespace SkillBridge.Business.Model.Db
{
    public interface IProgram
    {
        int Id { get; set; }
        string ProgramName { get; set; }
        string OrganizationName { get; set; }
        int OrganizationId { get; set; }
        OrganizationModel Organization { get; set; }
        bool IsActive { get; set; }
        DateTime DateDeactivated { get; set; }
        string LhnIntakeTicketId { get; set; } // LHN Intake Ticket Number
        bool HasIntake { get; set; } // Do we have a completed QuestionPro intake form from them
        string IntakeFormVersion { get; set; } // Which version of the QuestionPro intake form did they fill out
        string QpIntakeSubmissionId { get; set; } // The ID of the QuestionPro intake form submission

        //public bool HasLocations { get; set; }      // From col N of master spreadsheet
        bool LocationDetailsAvailable { get; set; } // From col O of master spreadsheet

        bool HasConsent { get; set; }
        string QpLocationSubmissionId { get; set; }
        string LhnLocationTicketId { get; set; }
        bool HasMultipleLocations { get; set; }
        bool ReportingForm2020 { get; set; }
        DateTime DateAuthorized { get; set; } // Date the 
        string MouLink { get; set; } // URL link to actual MOU packet
        DateTime MouCreationDate { get; set; }
        DateTime MouExpirationDate { get; set; }
        bool Nationwide { get; set; }
        bool Online { get; set; }
        string ParticipationPopulations { get; set; } // Might want enum for this
        string DeliveryMethod { get; set; }
        string StatesOfProgramDelivery { get; set; }
        int ProgramDuration { get; set; }
        bool SupportCohorts { get; set; }
        string OpportunityType { get; set; }
        string JobFamily { get; set; }
        string ServicesSupported { get; set; }
        string EnrollmentDates { get; set; }
        DateTime DateCreated { get; set; } // Date program was created in system
        DateTime DateUpdated { get; set; } // Date program was last edited/updated in the system
        string CreatedBy { get; set; }
        string UpdatedBy { get; set; }
        string ProgramUrl { get; set; }
        bool ProgramStatus { get; set; } // false is disabled, true is enabled
        string AdminPocFirstName { get; set; }
        string AdminPocLastName { get; set; }
        string AdminPocEmail { get; set; }
        string AdminPocPhone { get; set; }
        string PublicPocName { get; set; }
        string PublicPocEmail { get; set; }
        string Notes { get; set; }
        bool ForSpouses { get; set; }
        int LegacyProgramId { get; set; }
        int LegacyProviderId { get; set; }
        List<ProgramJobFamily> ProgramJobFamilies { get; set; }
        List<ProgramParticipationPopulation> ProgramParticipationPopulations { get; set; }
        List<ProgramService> ProgramServices { get; set; }
        List<ProgramState> ProgramStates { get; set; }
        List<ProgramDeliveryMethod> ProgramDeliveryMethods { get; set; }
        List<ProgramTrainingPlan> ProgramTrainingPlans { get; set; }
    }

    public class ProgramModel : IProgram
    {
        [Key] public int Id { get; set; }

        [Required]
        [Display(Name = "Program/Program Office/Agency")]
        [JsonProperty("PROGRAM")]
        public string ProgramName { get; set; }

        public string OrganizationName { get; set; }
        public int OrganizationId { get; set; }

        [JsonIgnore]
        [ForeignKey("OrganizationId")]
        public virtual OrganizationModel Organization { get; set; }

        [Required]
        [Display(Name = "Is Active")]
        [JsonIgnore]
        public bool IsActive { get; set; }

        [Display(Name = "Date Deactivated")]
        [JsonIgnore]
        public DateTime DateDeactivated { get; set; }

        [Display(Name = "LHN Intake Ticket Id")]
        public string LhnIntakeTicketId { get; set; } = ""; // LHN Intake Ticket Number

        [Display(Name = "Has Intake")]
        public bool HasIntake { get; set; } // Do we have a completed QuestionPro intake form from them

        [Display(Name = "Intake Form Version")]
        public string IntakeFormVersion { get; set; } =
            ""; // Which version of the QuestionPro intake form did they fill out

        [Display(Name = "QP Intake Submission Id")]
        public string QpIntakeSubmissionId { get; set; } = ""; // The ID of the QuestionPro intake form submission

        [Display(Name = "Location Details Available")]
        [JsonProperty("LOCATIONDETAILSAVAILABLE")]
        //public bool HasLocations { get; set; }      // From col N of master spreadsheet
        public bool LocationDetailsAvailable { get; set; } // From col O of master spreadsheet

        [Display(Name = "Has Consent")] public bool HasConsent { get; set; }

        [Display(Name = "Qp Location Submission Id")]
        public string QpLocationSubmissionId { get; set; } = "";

        [Display(Name = "LHN Location Ticket Id")]
        public string LhnLocationTicketId { get; set; } = "";

        [Display(Name = "Has Multiple Locations")]
        public bool HasMultipleLocations { get; set; }

        [Display(Name = "Reporting Form 2020")]
        public bool ReportingForm2020 { get; set; }

        [Required]
        [Display(Name = "Date Authorized")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime DateAuthorized { get; set; } // Date the 

        [Required]
        [Display(Name = "MOU Link")]
        [DataType(DataType.Url)]
        [RegularExpression(
            @"(https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|www\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9]+\.[^\s]{2,}|www\.[a-zA-Z0-9]+\.[^\s]{2,})",
            ErrorMessage = "Please provide a valid URL, you may need to include http:// or https://")]
        public string MouLink { get; set; } = ""; // URL link to actual MOU packet

        [Display(Name = "MOU Creation Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime MouCreationDate { get; set; }

        [Display(Name = "MOU Expiration Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime MouExpirationDate { get; set; }

        [JsonProperty("NATIONWIDE")] public bool Nationwide { get; set; }
        [JsonProperty("ONLINE")] public bool Online { get; set; }

        [Display(Name = "Participation Populations")]
        public string ParticipationPopulations { get; set; } // Might want enum for this

        [Display(Name = "Delivery Method")]
        [JsonProperty("DELIVERYMETHOD")]
        public string DeliveryMethod { get; set; }

        [Display(Name = "States Of Program Delivery")]
        [JsonProperty("STATES")]
        public string StatesOfProgramDelivery { get; set; } = "";

        [Required]
        [Display(Name = "Program Duration")]
        [JsonProperty("PROGRAMDURATION")]
        public int ProgramDuration { get; set; }

        [Display(Name = "Support Cohorts")]
        [JsonProperty("COHORTS")]
        public bool SupportCohorts { get; set; }

        [Required]
        [Display(Name = "Opportunity Type")]
        [JsonProperty("OPPORTUNITYTYPE")]
        public string OpportunityType { get; set; }

        [Display(Name = "Job Family")]
        [JsonProperty("JOBFAMILY")]
        public string JobFamily { get; set; } = "";

        [Display(Name = "Services Supported")] public string ServicesSupported { get; set; } = "";
        [Display(Name = "Enrollment Dates")] public string EnrollmentDates { get; set; } = "";

        [Display(Name = "Date Created")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime DateCreated { get; set; } // Date program was created in system

        [Display(Name = "Date Updated")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime DateUpdated { get; set; } // Date program was last edited/updated in the system

        [Display(Name = "Created By")] public string CreatedBy { get; set; }
        [Display(Name = "Updated By")] public string UpdatedBy { get; set; }

        [Display(Name = "Program URL")]
        [DataType(DataType.Url)]
        [RegularExpression(
            @"(https?:\/\/(?:www[a-zA-Z0-9]\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|www\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9]+\.[^\s]{2,}|www\.[a-zA-Z0-9]+\.[^\s]{2,}|https?:\/\/www\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,})",
            ErrorMessage = "Please provide a valid URL, you may need to include http:// or https://")]
        public string ProgramUrl { get; set; } = "";

        [Display(Name = "Program Status")]
        public bool ProgramStatus { get; set; } // false is disabled, true is enabled

        [Required]
        [Display(Name = "Admin POC First Name")]
        public string AdminPocFirstName { get; set; }

        [Required]
        [Display(Name = "Admin POC Last Name")]
        public string AdminPocLastName { get; set; }

        [Required]
        [Display(Name = "Admin POC Email")]
        [RegularExpression(@"^(.+)@(.{2,})\.(.{2,})$", ErrorMessage = "Please provide a valid email address")]
        public string AdminPocEmail { get; set; }

        [Required]
        [Display(Name = "Admin POC Phone Number")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$",
            ErrorMessage = "Not a valid phone number")]
        public string AdminPocPhone { get; set; }

        [Required]
        [Display(Name = "Public POC Name")]
        public string PublicPocName { get; set; }

        [Required]
        [Display(Name = "Public POC Email")]
        [RegularExpression(@"^(.+)@(.{2,})\.(.{2,})$", ErrorMessage = "Please provide a valid email address")]
        public string PublicPocEmail { get; set; }

        public string Notes { get; set; } = "";
        [Display(Name = "For Spouses")]
        public bool ForSpouses { get; set; }
        public int LegacyProgramId { get; set; }
        public int LegacyProviderId { get; set; }

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

        [JsonIgnore] public virtual List<ProgramTrainingPlan> ProgramTrainingPlans { get; set; }
    }
}
