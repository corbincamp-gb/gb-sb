using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SkillBridge_System_Prototype.Models
{
    public class SB_SiteConfiguration
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "Notification Type")]
        public int NotificationType { get; set; }   // This number determines if the notification is shown, and if it is, which notification type is it? Warning/Error/Success...
        [Display(Name = "Notification Message (HTML is allowed, script and style tags will be removed upon saving)")]
        public string NotificationHTML { get; set; }
    }
}
