using System.ComponentModel.DataAnnotations;

namespace SkillBridge.Business.Model.Db
{
    public class UserRoleModel
    {
        public string UserId { get; set; }
        public string RoleId { get; set; }
        [Required]
        public string UserName { get; set; }
        public bool IsSelected { get; set; }
    }
}
