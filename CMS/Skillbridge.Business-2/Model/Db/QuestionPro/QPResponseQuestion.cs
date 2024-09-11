using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Skillbridge.Business.Model.Db.QuestionPro
{
    [Table("QPResponseQuestions")]
    public class QPResponseQuestion
    {
        [Key]
        public int Id { get; set; }
        public int ResponseId { get; set; }
        public int SurveyId { get; set; }
        public int QuestionId { get; set; }
        public string QuestionDescription { get; set; }
        public string QuestionCode { get; set; }
        public string QuestionText { get; set; }
    }
}
