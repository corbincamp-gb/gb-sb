using System.ComponentModel.DataAnnotations;

namespace SkillBridge.Business.Model.Db
{
    public class CreateRoleModel
    {
        [Required]
        public string RoleName { get; set; }
    }
}
