using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace Skillbridge.Business.Util.SMTP
{
    public class SMTP : IEmailSender
    {
        public SMTP(IOptions<SMTPOptions> options)
        {
           Options = options.Value;
        }

        public SMTPOptions Options { get; set; }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            return Execute(email, subject, message);
        }

        public Task Execute(string to, string subject, string message)
        {
            // create message
            var email = new MimeMessage
            {
                Sender = MailboxAddress.Parse(Options.SenderEmail)
            };
            if (!string.IsNullOrEmpty(Options.SenderName))
                email.Sender.Name = Options.SenderName;
            email.From.Add(email.Sender);
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Html) { Text = message };

            // send email
            using (var smtp = new SmtpClient())
            {
                smtp.Connect(Options.Server, Options.Port, Options.SecureSocketOptions);
                smtp.Authenticate(Options.Account, Options.Password ??string.Empty);
                smtp.Send(email);
                smtp.Disconnect(true);
            }

            return Task.FromResult(true);
        }
    }
}
