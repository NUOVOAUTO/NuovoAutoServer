using Microsoft.Extensions.Options;

using NuovoAutoServer.Shared;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace NuovoAutoServer.Services.EmailNotification
{
    public class EmailNotificationService
    {
        private readonly EmailSettings _emailSettings;
        private readonly AppSettings _appSettings;

        public EmailNotificationService(IOptions<EmailSettings> emailSettings, IOptions<AppSettings> appSettings)
        {
            _emailSettings = emailSettings.Value;
            _appSettings = appSettings.Value;
        }

        public async Task SendEmailAsync(EmailRecipients recipients, string templateKey, object model)
        {
            if (!_emailSettings.Templates.TryGetValue(templateKey, out var template))
            {
                throw new ArgumentException($"Template '{templateKey}' not found.");
            }

            var subject = ReplacePlaceholders(template.Subject, model);
            var body = ReplacePlaceholders(template.Body, model);

            using var message = new MailMessage
            {
                From = new MailAddress(_appSettings.EmailConfig.SenderEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            // Add recipients from the method parameter
            AddRecipients(message, recipients);

            // Add recipients from the template
            if (template.Recipients != null)
            {
                AddRecipients(message, template.Recipients);
            }

            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Host = _appSettings.EmailConfig.SenderHost;
            smtpClient.Port = 587;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(_appSettings.EmailConfig.SenderEmail, _appSettings.EmailConfig.SenderPwd);
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.EnableSsl = true;
            await smtpClient.SendMailAsync(message);
        }

        private void AddRecipients(MailMessage message, EmailRecipients recipients)
        {
            if (recipients?.To != null)
            {
                foreach (var to in recipients.To)
                {
                    message.To.Add(to);
                }
            }

            if (recipients?.CC != null)
            {
                foreach (var cc in recipients.CC)
                {
                    message.CC.Add(cc);
                }
            }

            if (recipients?.BCC != null)
            {
                foreach (var bcc in recipients.BCC)
                {
                    message.Bcc.Add(bcc);
                }
            }
        }

        private string ReplacePlaceholders(string template, object model)
        {
            var properties = model.GetType().GetProperties();
            foreach (var property in properties)
            {
                var placeholder = $"{{{property.Name}}}";
                var value = property.GetValue(model)?.ToString() ?? string.Empty;
                template = template.Replace(placeholder, value);
            }
            return template;
        }
    }
}
