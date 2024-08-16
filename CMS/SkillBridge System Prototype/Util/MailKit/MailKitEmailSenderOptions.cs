using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Security;

namespace SkillBridge_System_Prototype.Util.MailKit
{    
    public class MailKitEmailSenderOptions
    {
        public MailKitEmailSenderOptions()
        {
            Host_SecureSocketOptions = SecureSocketOptions.Auto;
        }

        public string Host_Address { get; set; }

        public int Host_Port { get; set; }

        public string Host_Username { get; set; }

        public string Host_Password { get; set; }

        public SecureSocketOptions Host_SecureSocketOptions { get; set; }

        public string Sender_EMail { get; set; }

        public string Sender_Name { get; set; }
    }
}
