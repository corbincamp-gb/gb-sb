using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Text.Json.Serialization;

namespace SkillBridge_System_Prototype.Models.TrainingPlans
{
    [Table("TrainingPlanInstructionalMethods")]
    public class TrainingPlanInstructionalMethod
    {
        [Key]
        public int Id { get; set; }
        public int TrainingPlanId { get; set; }
        public int InstructionalMethodId { get; set; }
        [JsonIgnore]
        public virtual InstructionalMethod InstructionalMethod { get; set; }
        public string OtherText { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreateBy { get; set; }
    }
}
