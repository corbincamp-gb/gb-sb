using System.ComponentModel.DataAnnotations;

namespace Skillbridge.Business.Model.Db
{
    public class CreateOrganizationModelView : IValidatableObject
    {
        //Organization
        [Required]
        public int Mou_Id { get; set; }
        public bool Is_MOU_Parent { get; set; }
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
        [Required]
        [Display(Name = "POC Last Name")]
        public string Poc_Last_Name { get; set; }
        [Required]
        [EmailAddress]
        [Display(Name = "POC Email")]
        public string Poc_Email { get; set; }
        [Required]
        [Display(Name = "POC Phone")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid phone number")]
        public string Poc_Phone { get; set; }
        [Display(Name = "Organization URL")]
        [DataType(DataType.Url)]
        [RegularExpression(@"(https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|www\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9]+\.[^\s]{2,}|www\.[a-zA-Z0-9]+\.[^\s]{2,})", ErrorMessage = "Please provide a valid URL, you may need to include http:// or https://")]
        public string? Organization_Url { get; set; }
        [Required]
        [Display(Name = "Organization Type")]
        public int Organization_Type { get; set; }
        public string Notes { get; set; }

        //MOU
        [Display(Name = "Creation Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime Creation_Date { get; set; }
        [Display(Name = "Expiration Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime Expiration_Date { get; set; }
        [Display(Name = "MOU URL (including protocol)")]
        [DataType(DataType.Url)]
        [RegularExpression(@"(https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|www\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9]+\.[^\s]{2,}|www\.[a-zA-Z0-9]+\.[^\s]{2,})", ErrorMessage = "Please provide a valid URL, you may need to include http:// or https://")]
        public string Url { get; set; }
        public string Service { get; set; }
        public bool Is_OSD { get; set; }

        // For mous dropdown
        public List<MouModel> Mous { get; set; }
        //public string Organization_Name { get; set; }   // Update database on ingest
        //public int Legacy_Provider_Id { get; set; }   // Update database on ingest
        //
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // If checkbox marked, were creating a new mou
            if(Is_MOU_Parent == true)
            {
                if (Mou_Id == null || Mou_Id == -1)
                {
                    yield return new ValidationResult("Parent MOU must be selected.");
                }
            }
        }
    }
}