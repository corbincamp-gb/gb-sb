using MailKit.Security;

namespace Skillbridge.Business.Util.SMTP

{
    public class SMTPOptions
    {
        public SMTPOptions()
        {
            SecureSocketOptions = SecureSocketOptions.Auto;
        }

        public string Server { get; set; }

        public int Port { get; set; }
        //public int Host_Port => Port;

        public string Account { get; set; }
        //public string Host_Username => Account;

        public string Password { get; set; }
//        public string Host_Password => Password;

        public SecureSocketOptions SecureSocketOptions { get; set; }

        public string SenderEmail { get; set; }
  //      public string Sender_EMail => SenderEmail;

        public string SenderName { get; set; }
    //    public string Sender_Name => SenderName;
    }
}