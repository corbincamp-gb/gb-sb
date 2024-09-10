using System.ComponentModel.DataAnnotations;

namespace Skillbridge.Business.Model.Db
{
    public class CreateRoleModel
    {
        [Required]
        public string RoleName { get; set; }
    }
}
