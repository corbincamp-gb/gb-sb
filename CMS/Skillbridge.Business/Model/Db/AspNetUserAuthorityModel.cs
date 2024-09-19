using System.ComponentModel.DataAnnotations.Schema;

namespace SkillBridge.Business.Model.Db;

[Table("AspNetUserAuthorities")]
public class AspNetUserAuthorityModel
{
    public int Id { get; set; }
    public string ApplicationUserId { get; set; }
    [ForeignKey("OrganizationModel")]
    public int OrganizationId { get; set; }
    public virtual OrganizationModel Organization { get; set; }
    [ForeignKey("ProgramModel")]
    public int? ProgramId { get; set; }
    public virtual ProgramModel? Program { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; }
}