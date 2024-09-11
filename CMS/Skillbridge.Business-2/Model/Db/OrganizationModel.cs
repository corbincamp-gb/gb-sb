using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Skillbridge.Business.Model.Db
{
    [Table("Organizations")]
    public class OrganizationModel
    {
        [Key]
        public int Id { get; set; }  // auto-increment
        //[ForeignKey("SB_Mou")]
        public int Mou_Id { get; set; }     // This is the OSD level MOU that this organization operates under
        //public SB_Mou SB_Mou { get; set; }
        [Required]
        public bool Is_MOU_Parent { get; set; }
        [Required]
        [Display(Name = "Is Active")]
        [JsonIgnore]
        public bool Is_Active { get; set; }
        [Display(Name = "Date Deactivated")]
        [JsonIgnore]
        public DateTime Date_Deactivated { get; set; }
        public string Parent_Organization_Name { get; set; }
        [Required]
        [Display(Name = "Organization Name")]
        public string Name { get; set; }
        [Required]
        public string Poc_First_Name { get; set; }
        [Required]
        public string Poc_Last_Name { get; set; }
        [Required]
        [RegularExpression(@"^(.+)@(.{2,})\.(.{2,})$", ErrorMessage = "Please provide a valid email address")]
        public string Poc_Email { get; set; }
        [Required]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid phone number")]
        public string Poc_Phone { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        [JsonIgnore]
        public DateTime Date_Created { get; set; }  // Date org was created in system
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        [JsonIgnore]
        public DateTime Date_Updated { get; set; }  // Date org was last edited/updated in the system.
        [JsonIgnore]
        public string Created_By { get; set; }
        [JsonIgnore]
        public string Updated_By { get; set; }
        [DataType(DataType.Url)]
        [RegularExpression(@"(https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|www\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9]+\.[^\s]{2,}|www\.[a-zA-Z0-9]+\.[^\s]{2,})", ErrorMessage = "Please provide a valid URL, you may need to include http:// or https://")]
        public string Organization_Url { get; set; }
        [Required]
        public int Organization_Type { get; set; }
        public string States_Of_Program_Delivery { get; set; }
        public string Notes { get; set; }
        [JsonProperty("Provider Unique Id")]
        public int Legacy_Provider_Id { get; set; }
        [JsonIgnore]
        public int Legacy_MOU_Id { get; set; }  // This legacy ID will reference the unique provider ID of the ORg responsible for an MOU, derived from the Provider Unique ID

        public List<ProgramModel> Programs { get; set; }

    }
}
