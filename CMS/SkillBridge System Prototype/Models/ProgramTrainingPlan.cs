using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace SkillBridge_System_Prototype.Models
{
    [Table("ProgramTrainingPlans")]
    public class ProgramTrainingPlan
    {
        [Key]
        public int Id { get; set; }
        public int ProgramId { get; set; }
        [ForeignKey("ProgramId")]
        public virtual SB_Program SB_Program { get; set; }
        public int TrainingPlanId { get; set; }
        [JsonIgnore]
        public virtual SkillBridge_System_Prototype.Models.TrainingPlans.TrainingPlan TrainingPlan { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreateBy { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ActivationChangeDate { get; set; }
    }
}
