using System.ComponentModel.DataAnnotations;

namespace SkillBridge.Business.Model.Db
{
    public class EditUserModel
    {
        public List<string> Claims { get; set; }
        public IList<string> Roles { get; set; }

        public string Id { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
		[RegularExpression(@"^(.+)@(.{2,})\.(.{2,})$", ErrorMessage = "Please provide a valid email address")]
		public string Email { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        [Required]
        public string Notes { get; set; }
        [Required]
        public string OrganizationId { get; set; }
        public string ProgramId { get; set; }

        public bool MustChangePassword { get; set; }

        public EditUserModel()
        {
            Claims = new List<string>();
            Roles = new List<string>();
        }
    }
}
