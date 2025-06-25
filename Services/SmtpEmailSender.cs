using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace NLI_POS.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public IFormFile Attachment { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public SmtpEmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor)
        {
            Options = optionsAccessor.Value;
        }

        public AuthMessageSenderOptions Options { get; } //set only via Secret Manager

        public Task SendEmailAsync(string email, string subject, string message)
        {
            // return Execute(Options.SendGridKey, subject, message, email);
            return EmailExec(subject, message, email);
        }

        public Task EmailExec(string subject, string message, string email)
        {
            Email = "pos.nlip@gmail.com";
            To = email; // "armand.nlip@gmail.com";

            using (MailMessage mm = new MailMessage(Email, To))
            {
                mm.From = new MailAddress(Email, "NobleLife POS");
                mm.Subject = subject;
                mm.Body = message;

                if (Attachment != null)
                {
                    if (Attachment.Length > 0)
                    {
                        string fileName = Path.GetFileName(Attachment.FileName);
                        mm.Attachments.Add(new Attachment(Attachment.OpenReadStream(), fileName));
                    }
                }
                mm.IsBodyHtml = true;
                using (SmtpClient smtp = new SmtpClient())
                {
                    Password = "kqqjvtwtsgwckbnc"; //"N0blelife2@22"; //"Noblelife2006";                    
                    smtp.Host = "smtp.gmail.com";
                    smtp.EnableSsl = true;
                    smtp.UseDefaultCredentials = false;
                    NetworkCredential NetworkCred = new NetworkCredential(Email, Password);
                    smtp.Credentials = NetworkCred;
                    smtp.Port = 587;
                    smtp.Send(mm);
                }

            }
            return Task.CompletedTask;
        }
    }
}
