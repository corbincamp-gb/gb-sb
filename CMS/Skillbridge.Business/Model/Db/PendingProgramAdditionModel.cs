namespace SkillBridge.Business.Model.Db
{
    public class PendingProgramAdditionModel
    {
        [System.ComponentModel.DataAnnotations.Key]
        public int Id { get; set; }  // auto-increment, this is the ID of the actual change in the pending change table
        public int Program_Id { get; set; }    ///This is the actual ID for the referenced Program that the change will happen on
        public bool Is_Active { get; set; }
        public string Program_Name { get; set; }
        public string Organization_Name { get; set; }
        public int Organization_Id { get; set; }
        public string Lhn_Intake_Ticket_Id { get; set; }  // LHN Intake Ticket Number
        public bool Has_Intake { get; set; }        // Do we have a completed QuestionPro intake form from them
        public string Intake_Form_Version { get; set; }  // Which version of the QuestionPro intake form did they fill out
        public string Qp_Intake_Submission_Id { get; set; } // The ID of the QuestionPro intake form submission
        //public bool Has_Locations { get; set; }      // From col N of master spreadsheet
        public bool Location_Details_Available { get; set; } // From col O of master spreadsheet
        public bool Has_Consent { get; set; }
        public string Qp_Location_Submission_Id { get; set; }
        public string Lhn_Location_Ticket_Id { get; set; }
        public bool Has_Multiple_Locations { get; set; }
        public bool Reporting_Form_2020 { get; set; }
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [System.ComponentModel.DataAnnotations.DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime Date_Authorized { get; set; }  // Date the 
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Url)]
        [System.ComponentModel.DataAnnotations.RegularExpression(@"(https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|www\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9]+\.[^\s]{2,}|www\.[a-zA-Z0-9]+\.[^\s]{2,})", ErrorMessage = "Please provide a valid URL, you may need to include http:// or https://")]
        public string Mou_Link { get; set; }       // URL link to actual MOU packet
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [System.ComponentModel.DataAnnotations.DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime Mou_Creation_Date { get; set; }
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [System.ComponentModel.DataAnnotations.DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime Mou_Expiration_Date { get; set; }
        public bool Nationwide { get; set; }
        public bool Online { get; set; }
        public string Participation_Populations { get; set; }  // Might want enum for this
        public string Delivery_Method { get; set; }
        public string States_Of_Program_Delivery { get; set; }
        public int Program_Duration { get; set; }
        public bool Support_Cohorts { get; set; }
        public string Opportunity_Type { get; set; }
        public string Job_Family { get; set; }
        public string Services_Supported { get; set; }
        public string Enrollment_Dates { get; set; }
        public DateTime Date_Created { get; set; } // Date program was created in system
        public DateTime Date_Updated { get; set; } // Date program was last edited/updated in the system
        public string Created_By { get; set; }
        public string Updated_By { get; set; }
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Url)]
        [System.ComponentModel.DataAnnotations.RegularExpression(@"(https?:\/\/(?:www[a-zA-Z0-9]\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|www\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9]+\.[^\s]{2,}|www\.[a-zA-Z0-9]+\.[^\s]{2,}|https?:\/\/www\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,})", ErrorMessage = "Please provide a valid URL, you may need to include http:// or https://")]
        public string Program_Url { get; set; }
        public bool Program_Status { get; set; } // 0 is disabled, 1 is enabled
        public string Admin_Poc_First_Name { get; set; }
        public string Admin_Poc_Last_Name { get; set; }
		[System.ComponentModel.DataAnnotations.RegularExpression(@"^(.+)@(.{2,})\.(.{2,})$", ErrorMessage = "Please provide a valid email address")]
		public string Admin_Poc_Email { get; set; }
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.PhoneNumber)]
        [System.ComponentModel.DataAnnotations.RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid phone number")]
        public string Admin_Poc_Phone { get; set; }
        public string Public_Poc_Name { get; set; }
		[System.ComponentModel.DataAnnotations.RegularExpression(@"^(.+)@(.{2,})\.(.{2,})$", ErrorMessage = "Please provide a valid email address")]
		public string Public_Poc_Email { get; set; }
        public string Notes { get; set; }
        public bool For_Spouses { get; set; }
        public int Legacy_Program_Id { get; set; }
        public int Legacy_Provider_Id { get; set; }
        public int Pending_Change_Status { get; set; }
        public bool Requires_OSD_Review { get; set; }
        public string Last_Admin_Action_User { get; set; }
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [System.ComponentModel.DataAnnotations.DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? Last_Admin_Action_Time { get; set; }
        public string Last_Admin_Action_Type { get; set; }
        public string Rejection_Reason { get; set; }

        public string? SerializedTrainingPlan { get; set; }
    }
}
