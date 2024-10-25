﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SkillBridge.Business.Model.Db.TrainingPlans
{
    [Table("TrainingPlanInstructionalMethods")]
    public class TrainingPlanInstructionalMethod
    {
        [Key]
        public int Id { get; set; }
        public int TrainingPlanId { get; set; }
        public int InstructionalMethodId { get; set; }
        [global::Newtonsoft.Json.JsonIgnore]
        public virtual InstructionalMethod InstructionalMethod { get; set; }
        public string OtherText { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreateBy { get; set; }
    }
}
