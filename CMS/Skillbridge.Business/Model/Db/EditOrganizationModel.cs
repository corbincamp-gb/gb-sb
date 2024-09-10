using System.ComponentModel.DataAnnotations;

namespace Skillbridge.Business.Model.Db
{
    public class EditOrganizationModel
    {
        public string Id { get; set; }
        [Required]
        [Display(Name = "Organization Name")]
        public string Name { get; set; }
        [Required]
        [Display(Name = "Is Active")]
        public bool Is_Active { get; set; }
        [Display(Name = "Date Deactivated")]
        public DateTime Date_Deactivated { get; set; }
        [Required]
        [Display(Name = "POC First Name")]
        public string Poc_First_Name { get; set; }
        [Display(Name = "POC Last Name")]
        public string Poc_Last_Name { get; set; }
        [Required]
        [Display(Name = "POC E-mail Address")]
		[RegularExpression(@"^(.+)@(.{2,})\.(.{2,})$", ErrorMessage = "Please provide a valid email address")]
		public string Poc_Email { get; set; }
        [Required]
        [Display(Name = "POC Phone Number")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid phone number")]
        public string Poc_Phone { get; set; }
        [Display(Name = "Date Created")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime Date_Created { get; set; }  // Date org was created in system
        [Display(Name = "Date Updated")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime Date_Updated { get; set; }  // Date org was last edited/updated in the system.
        public string Created_By { get; set; }
        public string Updated_By { get; set; }
        [Display(Name = "Organization Web Site")]
        [DataType(DataType.Url)]
        [RegularExpression(@"(https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|www\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9]+\.[^\s]{2,}|www\.[a-zA-Z0-9]+\.[^\s]{2,})", ErrorMessage = "Please provide a valid URL, you may need to include http:// or https://")]
        public string Organization_Url { get; set; }
        [Required]
        [Display(Name = "Type of Organization")]
        public int Organization_Type { get; set; }
        [Display(Name = "States Of Program Delivery")]
        public string States_Of_Program_Delivery { get; set; }
        public string Notes { get; set; }
        public int Legacy_Provider_Id { get; set; }

        public List<string> Pending_Fields { get; set; }
        public int Pending_Change_Status { get; set; }

        [Display(Name = "Rejection Reason")]
        public string Rejection_Reason { get; set; }
    }
}
