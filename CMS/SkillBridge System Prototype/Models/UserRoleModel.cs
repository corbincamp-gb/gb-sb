using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SkillBridge_System_Prototype.Models
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
