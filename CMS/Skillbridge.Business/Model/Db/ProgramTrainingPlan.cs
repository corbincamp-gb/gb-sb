using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using SkillBridge.Business.Model.Db.TrainingPlans;

namespace SkillBridge.Business.Model.Db
{
    [Table("ProgramTrainingPlans")]
    public class ProgramTrainingPlan
    {
        [Key]
        public int Id { get; set; }
        public int ProgramId { get; set; }
        [ForeignKey("ProgramId")]
        public virtual ProgramModel Program { get; set; }
        public int TrainingPlanId { get; set; }
        [JsonIgnore]
        public virtual TrainingPlan TrainingPlan { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreateBy { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ActivationChangeDate { get; set; }
    }
}
