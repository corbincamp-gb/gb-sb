using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillBridge.Business.Model.Db.TrainingPlans
{
    [Table("TrainingPlanLengths")]
    public class TrainingPlanLength
    {
        [Key]
        public int Id { get; set; }
        public string DisplayText { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreateBy { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdateBy { get; set; }
        public bool IsActive { get; set; }
    }
}
