using System.ComponentModel.DataAnnotations;

namespace SkillBridge.Business.Model.Db
{
    public class EditTrainingPlanModel
    {
        public string Id { get; set; }
        [Required]
        [Display(Name = "Job Title")]
        public string JobTitle { get; set; }
        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }
        [Required]
        [Display(Name = "Length")]
        public string Length { get; set; }
        [Required]
        [Display(Name = "Objective Alignment")]
        public string ObjectiveAlignment { get; set; }
        [Required]
        [Display(Name = "Time Breakdown")]
        public string TimeBreakdown { get; set; }
        [Required]
        [Display(Name = "Instructional Modules")]
        public List<string> InstructionalModules { get; set; }
        [Required]
        [Display(Name = "Instructional Methods")]
        public string InstructionalMethods { get; set; }
        [Display(Name = "Other Instructional Method")]
        public string OtherInstructionalMethod { get; set; }
        [Required]
        [Display(Name = "Who Delivers")]
        public string WhoDelivers { get; set; }
        [Required]
        [Display(Name = "Grading Rubric")]
        public string GradingRubric { get; set; }
        [Required]
        [Display(Name = "Credentials Earned")]
        public string CredentialsEarned { get; set; }
    }
}
