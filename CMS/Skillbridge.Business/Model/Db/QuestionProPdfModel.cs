using System.ComponentModel.DataAnnotations;

namespace SkillBridge.Business.Model.Db
{
    public class QuestionProPdfModel
    {
        [Key]
        public int Id { get; set; }

        public string ZohoTicketId { get; set; }
        
        public DateTime CreateDate { get; set; }
        
        public string FileName { get; set; }
        
        public string TimeStamp { get; set; }
    }
}
