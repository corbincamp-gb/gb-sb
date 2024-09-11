using System.ComponentModel.DataAnnotations;

namespace SkillBridge.Business.Model.Db
{
    public class MouModel
    {
        [Key]
        public int Id { get; set; }  // auto-increment
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime Creation_Date { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime Expiration_Date { get; set; }
        [DataType(DataType.Url)]
        [RegularExpression(@"(https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|www\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9]+\.[^\s]{2,}|www\.[a-zA-Z0-9]+\.[^\s]{2,})", ErrorMessage = "Please provide a valid URL, you may need to include http:// or https://")]
        public string Url { get; set; }
        public string Service { get; set; }
        public bool Is_OSD { get; set; }
        public string Organization_Name { get; set; }   // Update database on ingest
        public int Legacy_Provider_Id { get; set; }   // Update database on ingest          
        //public bool Is_Grandfathered { get; set; }
        public DateTime? NotificationDate30Days { get; set; }
        public DateTime? NotificationDate60Days { get; set; }
        public DateTime? NotificationDate90Days { get; set; }
    }
}
