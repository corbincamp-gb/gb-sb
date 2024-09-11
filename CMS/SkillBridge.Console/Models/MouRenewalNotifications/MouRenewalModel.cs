using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SkillBridgeConsoleApp.Models.MouRenewalNotifications
{
    public class MouRenewalModel
    {
        [Key]
        public int Mou_Id { get; set; }
        public string Poc_First_Name { get; set; }
        public string Poc_Last_Name { get; set; }
        public string Poc_Email { get; set; }
        public string Organization_Name { get; set; }
        public DateTime Expiration_Date { get; set; }
        public int DaysTilExpiry { get; set; }
        public string? ZohoTicketId { get; set; }
        public DateTime? NotificationDate90Days { get; set; }
        public DateTime? NotificationDate60Days { get; set; }
        public DateTime? NotificationDate30Days { get; set; }
    }
}