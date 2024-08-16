﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SkillBridge_System_Prototype.Models
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
