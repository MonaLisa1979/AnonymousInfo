using AnonymousInfo.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace AnonymousInfo.Services
{
    public class MailService : IMailService
    {
        private readonly GmailSettings gmailSettings;
        private readonly ILogger<MailService> logger;

        public MailService(IOptions<GmailSettings> gmailSettings, ILogger<MailService> logger)
        {
            this.gmailSettings = gmailSettings.Value;
            this.logger = logger;
        }

        public bool Send(string from, string to, string subject, string content, Dictionary<string, Stream> attachments)
        {
            try
            {
                var smtpClient = new SmtpClient
                {
                    Host = gmailSettings.Host,
                    Port = gmailSettings.Port,
                    EnableSsl = gmailSettings.SMTP.Starttls.Enable,
                    Credentials = new NetworkCredential(gmailSettings.Username, gmailSettings.Password),
                    UseDefaultCredentials = false
                };

                var mailMessage = new MailMessage(from, to)
                {
                    Subject = subject,
                    Body = content,
                    IsBodyHtml = false
                };

                if (attachments != null)
                {
                    foreach (var attachment in attachments)
                    {
                        Stream fileContent = attachment.Value;
                        mailMessage.Attachments.Add(new Attachment(fileContent, attachment.Key));
                    }
                }

                var task = smtpClient.SendMailAsync(mailMessage);

                task.Wait();
                return task.IsCompletedSuccessfully;
            }
            catch (Exception exc)
            {
                // Check if external services enabled https://www.google.com/settings/security/lesssecureapps
                this.logger.LogError(exc, $"Error occured while sending email. {exc.Message}");
                return false;
            }
            finally
            {
                if (attachments != null)
                {
                    foreach (var attachment in attachments)
                    {
                        attachment.Value.DisposeAsync();
                    }
                }
            }
        }
    }
}
