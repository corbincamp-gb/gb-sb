﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Text.Json.Serialization;

namespace SkillBridge_System_Prototype.Models.TrainingPlans
{
    [Table("TrainingPlanBreakdowns")]
    public class TrainingPlanBreakdown
    {
        [Key]
        public int Id { get; set; }
        public int TrainingPlanId { get; set; }
        public int RowId { get; set; }
        public string TrainingModuleTitle { get; set; }
        public string LearningObjective { get; set; }
        public decimal TotalHours { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreateBy { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdateBy { get; set; }
    }
}
