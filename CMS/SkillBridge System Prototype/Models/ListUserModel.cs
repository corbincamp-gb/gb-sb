using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SkillBridge_System_Prototype.Models
{
    public class ListUserModel
    {
        public ApplicationUser User { get; set; }
        public string RoleNames { get; set; }
    }
}
