using Microsoft.AspNetCore.Identity;

namespace SkillBridge.Business.Model.Db
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string? Notes { get; set; }
        public int? OrganizationId { get; set; }
        public int? ProgramId { get; set; }
        public bool MustChangePassword { get; set; }

        public const string MustChangePasswordClaimType = "http://userswithoutidentity/claims/mustchangepassword";
    }
}
