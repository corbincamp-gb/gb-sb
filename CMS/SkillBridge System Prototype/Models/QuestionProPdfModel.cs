using System;
using System.ComponentModel.DataAnnotations;

namespace SkillBridge_System_Prototype.Models
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
