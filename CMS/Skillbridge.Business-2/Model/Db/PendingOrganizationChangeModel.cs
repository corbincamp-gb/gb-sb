using System.ComponentModel.DataAnnotations;

namespace Skillbridge.Business.Model.Db
{
    public class PendingOrganizationChangeModel
    {
        public int Id { get; set; }  // auto-increment, this is the ID of the actual change in the pending change table
        public int Organization_Id { get; set; }    //This is the actual ID for the referenced Organization that the change will happen on
        public bool Is_Active { get; set; }
        public string Name { get; set; }
        public string Poc_First_Name { get; set; }
        public string Poc_Last_Name { get; set; }
		[RegularExpression(@"^(.+)@(.{2,})\.(.{2,})$", ErrorMessage = "Please provide a valid email address")]
		public string Poc_Email { get; set; }
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid phone number")]
        public string Poc_Phone { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime Date_Created { get; set; } = DateTime.Now;  // Date org was created in system
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime Date_Updated { get; set; } = DateTime.Now; // Date org was last edited/updated in the system.
        public string Created_By { get; set; }
        public string Updated_By { get; set; }
        [DataType(DataType.Url)]
        [RegularExpression(@"(https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|www\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9]+\.[^\s]{2,}|www\.[a-zA-Z0-9]+\.[^\s]{2,})", ErrorMessage = "Please provide a valid URL, you may need to include http:// or https://")]
        public string Organization_Url { get; set; }
        public int Organization_Type { get; set; }
        public string Notes { get; set; }
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
