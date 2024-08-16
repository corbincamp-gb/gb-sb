using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillBridge_System_Prototype.Models.QuestionPro
{
    [Table("QPPdfs")]
    public class QPPdf
    {
        [Key]
        public int Id { get; set; }
        public int ResponseId { get; set; }
        public string ZohoTicketId { get; set; }
        public DateTime CreateDate { get; set; }
        public string FileName { get; set; }
        public byte[] Pdf { get; set; }
    }
}
