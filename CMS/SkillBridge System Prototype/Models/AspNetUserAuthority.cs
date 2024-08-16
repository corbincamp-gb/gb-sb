using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace SkillBridge_System_Prototype.Models
{
    [Table("AspNetUserAuthorities")]
    public class AspNetUserAuthority
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; }
        [ForeignKey("SB_Organization")]
        public int OrganizationId { get; set; }
        public virtual SB_Organization SB_Organization { get; set; }
        [ForeignKey("SB_Program")]
        public int? ProgramId { get; set; }
        public virtual SB_Program? SB_Program { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
    }
}