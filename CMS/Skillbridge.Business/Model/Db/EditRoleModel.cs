using System.ComponentModel.DataAnnotations;

namespace Skillbridge.Business.Model.Db
{
    public class EditRoleModel
    {
        [Required]
        public string Id { get; set; }
        [Required (ErrorMessage="Role Name is required")]
        public string RoleName { get; set; }
        public List<string> Users { get; set; }

        public EditRoleModel()
        {
            Users = new List<string>();
        }
    }
}
