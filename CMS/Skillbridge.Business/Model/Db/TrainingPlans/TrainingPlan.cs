using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace SkillBridge.Business.Model.Db.TrainingPlans
{
    [Table("TrainingPlans")]
    public class TrainingPlan
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string JobTitle { get; set; }
        public string Description { get; set; }
        public int TrainingPlanLengthId { get; set; }
        [JsonIgnore]
        public TrainingPlanLength TrainingPlanLength { get; set; }
        public int BreakdownCount { get; set; }
        public string InstructionalModules { get; set; }
        public string WhoDelivers { get; set; }
        public string GradingRubric { get; set; }
        public string CredentialsEarned { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreateBy { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdateBy { get; set; }
        public bool IsActive { get; set; } = true;

        public List<TrainingPlanInstructionalMethod> TrainingPlanInstructionalMethods { get; set; } = new List<TrainingPlanInstructionalMethod>();
        public List<TrainingPlanBreakdown> TrainingPlanBreakdowns { get; set; } = new List<TrainingPlanBreakdown>();
        [JsonIgnore]
        public List<ProgramTrainingPlan> ProgramTrainingPlans { get; set; } = new List<ProgramTrainingPlan>();

        public string GetTrainingPlanInstructionalMethods()
        {
            var html = new System.Text.StringBuilder();

            for(var i = 0; i < TrainingPlanInstructionalMethods.Count; i++)
            {
                if (TrainingPlanInstructionalMethods[i].InstructionalMethod != null)
                {
                    html.AppendFormat(TrainingPlanInstructionalMethods[i].InstructionalMethod.DisplayText);

                    if (TrainingPlanInstructionalMethods[i].InstructionalMethod.IsOther && !string.IsNullOrWhiteSpace(TrainingPlanInstructionalMethods[i].OtherText))
                    {
                        html.AppendFormat($" - {TrainingPlanInstructionalMethods[i].OtherText}");
                    }
                }

                if (i < TrainingPlanInstructionalMethods.Count - 1)
                {
                    html.Append(", ");
                }
            }

            return html.ToString();
        }
    }
}
