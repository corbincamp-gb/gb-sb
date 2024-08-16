using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace SkillBridge_System_Prototype.Models
{
    public class ProgramModel
    {
        public SB_Program Program { get; set; }

        public TrainingPlans.TrainingPlan TrainingPlan { get; set; }
    }
}
