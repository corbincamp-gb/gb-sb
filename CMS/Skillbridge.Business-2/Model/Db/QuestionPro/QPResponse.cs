using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Skillbridge.Business.Model.Db.QuestionPro
{
    [Table("QPResponses")]
    public class QPResponse
    {
        [Key]
        public int Id { get; set; }
        public int ResponseId { get; set; }
        public int SurveyId { get; set; }
        public string SurveyName { get; set; }
        public bool Duplicate { get; set; }
        public string ResponseStatus { get; set; }
        public string ExternalReference { get; set; }
        public string ZohoTicketId { get; set; }
        public string IpAddress { get; set; }
        public string TimeStamp { get; set; }
        public int? TimeTaken { get; set; }
        public DateTime ImportDate { get; set; }
    }
}
