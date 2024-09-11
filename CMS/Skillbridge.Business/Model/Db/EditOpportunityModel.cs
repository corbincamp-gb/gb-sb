using System.ComponentModel.DataAnnotations;

namespace SkillBridge.Business.Model.Db
{
    public class EditOpportunityModel
    {
        public string Id { get; set; }
        [Display(Name = "Opportunity ID")]
        public int Opportunity_Id { get; set; }
        [Display(Name = "Group ID")]
        public int Group_Id { get; set; }
        [Display(Name = "Is Active")]
        public bool Is_Active { get; set; }
        [Display(Name = "Program/Program Office/Agency")]
        public string Program_Name { get; set; }
        [Display(Name = "Opportunity URL")]
        [DataType(DataType.Url)]
        [RegularExpression(@"(https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|www\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9]+\.[^\s]{2,}|www\.[a-zA-Z0-9]+\.[^\s]{2,})", ErrorMessage = "Please provide a valid URL, you may need to include http:// or https://")]
        public string Opportunity_Url { get; set; }
        [Display(Name = "Date Program Inititated")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime Date_Program_Initiated { get; set; }
        [Display(Name = "Date Created")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime Date_Created { get; set; } // Date opportunity was created in system
        [Display(Name = "Date Updated")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime Date_Updated { get; set; } // Date opportunity was last edited/updated in the system
        [Required]
        [Display(Name = "Employer POC Name")]
        public string Employer_Poc_Name { get; set; }
        [Required]
        [Display(Name = "Employer POC Email")]
		[RegularExpression(@"^(.+)@(.{2,})\.(.{2,})$", ErrorMessage = "Please provide a valid email address")]
		public string Employer_Poc_Email { get; set; }
        [Required]
        [Display(Name = "Training Duration")]
        public string Training_Duration { get; set; }
        [Required]
        [Display(Name = "Service")]
        public string Service { get; set; }
        [Display(Name = "Delivery Method")]
        public string Delivery_Method { get; set; }
        [Display(Name = "Multiple Locations")]
        public bool Multiple_Locations { get; set; }
        [Required]
        [Display(Name = "Program Type")]
        public string Program_Type { get; set; }
        [Display(Name = "Job Families")]
        public string Job_Families { get; set; }
        [Display(Name = "Participation Populations")]
        public string Participation_Populations { get; set; }
        [Display(Name = "Support Cohorts")]
        public bool Support_Cohorts { get; set; }
        [Display(Name = "Enrollment Dates")]
        public string Enrollment_Dates { get; set; }
        [Display(Name = "MOUs")]
        public bool Mous { get; set; }
        [Required]
        [Display(Name = "Number of Locations")]
        public int Num_Locations { get; set; }
        [Display(Name = "Installation")]
        public string Installation { get; set; }
        [Display(Name = "City")]
        public string City { get; set; }
        [Display(Name = "State")]
        public string State { get; set; }
        [Display(Name = "Zip")]
        [StringLength(5, MinimumLength = 5)]
        public string Zip { get; set; }
        [Required]
        [Display(Name = "Latitude")]
        public double Lat { get; set; }
        [Required]
        [Display(Name = "Longitude")]
        public double Long { get; set; }
        [Display(Name = "Nationwide")]
        public bool Nationwide { get; set; }
        [Display(Name = "Online")]
        public bool Online { get; set; }
        [Required]
        [Display(Name = "Summary Description")]
        public string Summary_Description { get; set; }
        [Required]
        [Display(Name = "Jobs Description")]
        public string Jobs_Description { get; set; }
        [Display(Name = "Links to Prospective Jobs")]
        public string Links_To_Prospective_Jobs { get; set; }
        [Display(Name = "Locations of Prospective Jobs by State")]
        public string Locations_Of_Prospective_Jobs_By_State { get; set; }
        [Display(Name = "Salary")]
        public string Salary { get; set; }
        [Display(Name = "Prospective Job Labor Demand")]
        public string Prospective_Job_Labor_Demand { get; set; }
        [Display(Name = "Target MOCs")]
        public string Target_Mocs { get; set; }
        [Display(Name = "Other Eligibility Factors")]
        public string Other_Eligibility_Factors { get; set; }
        [Required]
        [Display(Name = "Cost")]
        public string Cost { get; set; }
        [Required]
        [Display(Name = "Other")]
        public string Other { get; set; }
        [Display(Name = "Notes")]
        public string Notes { get; set; }
        [Display(Name = "Created By")]
        public string Created_By { get; set; }
        [Display(Name = "Updated By")]
        public string Updated_By { get; set; }
        [Display(Name = "For Spouses")]
        public bool For_Spouses { get; set; }
        [Display(Name = "Legacy Opportunity Id")]
        public int Legacy_Opportunity_Id { get; set; }
        [Display(Name = "Legacy Program Id")]
        public int Legacy_Program_Id { get; set; }
        [Display(Name = "Legacy Provider Id")]
        public int Legacy_Provider_Id { get; set; }

        public List<string> Pending_Fields { get; set; }
        public int Pending_Change_Status { get; set; }

        [Display(Name = "Rejection Reason")]
        public string Rejection_Reason { get; set; }

    }
}
