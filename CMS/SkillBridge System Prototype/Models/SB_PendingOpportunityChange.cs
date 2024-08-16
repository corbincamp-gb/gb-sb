using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SkillBridge_System_Prototype.Models
{
    public class SB_PendingOpportunityChange
    {
        [Key]
        public int Id { get; set; }  // auto-increment, this is the ID of the actual change in the pending change table
        public int Group_Id { get; set; }
        public int Organization_Id { get; set; }
        public int Opportunity_Id { get; set; }    ///This is the actual ID for the referenced Opportunity that the change will happen on
        public int Program_Id { get; set; }
        public bool Is_Active { get; set; }
        public string Program_Name { get; set; }
        [DataType(DataType.Url)]
        [RegularExpression(@"(https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|www\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9]+\.[^\s]{2,}|www\.[a-zA-Z0-9]+\.[^\s]{2,})", ErrorMessage = "Please provide a valid URL, you may need to include http:// or https://")]
        public string Opportunity_Url { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime Date_Program_Initiated { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime Date_Created { get; set; } // Date opportunity was created in system
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime Date_Updated { get; set; } // Date opportunity was last edited/updated in the system
        public string Employer_Poc_Name { get; set; }
		[RegularExpression(@"^(.+)@(.{2,})\.(.{2,})$", ErrorMessage = "Please provide a valid email address")]
		public string Employer_Poc_Email { get; set; }
        public string Training_Duration { get; set; }
        public string Service { get; set; }
        public string Delivery_Method { get; set; }
        public bool Multiple_Locations { get; set; }
        public string Program_Type { get; set; }
        public string Job_Families { get; set; }
        public string Participation_Populations { get; set; }
        public bool Support_Cohorts { get; set; }
        public string Enrollment_Dates { get; set; }
        public bool Mous { get; set; }
        public int Num_Locations { get; set; }
        public string Installation { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        [StringLength(5, MinimumLength = 5)]
        public string Zip { get; set; }
        public double Lat { get; set; }
        public double Long { get; set; }
        public bool Nationwide { get; set; }
        public bool Online { get; set; }
        public string Summary_Description { get; set; }
        public string Jobs_Description { get; set; }
        public string Links_To_Prospective_Jobs { get; set; }
        public string Locations_Of_Prospective_Jobs_By_State { get; set; }
        public string Salary { get; set; }
        public string Prospective_Job_Labor_Demand { get; set; }
        public string Target_Mocs { get; set; }
        public string Other_Eligibility_Factors { get; set; }
        public string Cost { get; set; }
        public string Other { get; set; }
        public string Notes { get; set; }
        public string Created_By { get; set; }
        public string Updated_By { get; set; }
        public bool For_Spouses { get; set; }
        public int Legacy_Opportunity_Id { get; set; }
        public int Legacy_Program_Id { get; set; }
        public int Legacy_Provider_Id { get; set; }
        public int Pending_Change_Status { get; set; }
        public bool Requires_OSD_Review { get; set; }
        public string Last_Admin_Action_User { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? Last_Admin_Action_Time { get; set; }
        public string Last_Admin_Action_Type { get; set; }
        public string Rejection_Reason { get; set; }
    }
}
