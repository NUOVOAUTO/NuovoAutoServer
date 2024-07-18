using Microsoft.Extensions.Options;

using NuovoAutoServer.Shared;

using SendGrid.Helpers.Mail;
using SendGrid;

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

        public async Task SendEmailAsync(EmailRecipients recipients, string templateKey, object[] modelArgs)
        {
            if (!_emailSettings.Templates.TryGetValue(templateKey, out var template))
            {
                throw new ArgumentException($"Template '{templateKey}' not found.");
            }

            var subject = ReplacePlaceholders(template.Subject, modelArgs);
            var body = template.Body.StartsWith("%") && template.Body.EndsWith("%")
                ? await LoadTemplateFromFileAsync(template.Body.Trim('%'), modelArgs)
                : ReplacePlaceholders(template.Body, modelArgs);

            using var message = new MailMessage
            {
                From = new MailAddress(_appSettings.EmailConfig.SenderEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            // Add recipients from the method parameter
            var recipientsList = new List<EmailRecipients>();
            recipientsList.Add(recipients);
            // Add recipients from the template
            if (template.Recipients != null)
            {
                recipientsList.Add(template.Recipients);
            }
            var (toEmails, ccEmails, bccEmails) = AddRecipients(recipientsList);

            var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(_appSettings.EmailConfig.SenderEmail, "Info");
            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, toEmails, subject, "", body);
            if (ccEmails.Any())
                msg.AddCcs(ccEmails);
            if (bccEmails.Any())
                msg.AddBccs(bccEmails);

            var response = await client.SendEmailAsync(msg);

            //SmtpClient smtpClient = new SmtpClient
            //{
            //    Host = _appSettings.EmailConfig.SenderHost,
            //    Port = 587,
            //    UseDefaultCredentials = false,
            //    Credentials = new NetworkCredential(_appSettings.EmailConfig.SenderEmail, _appSettings.EmailConfig.SenderPwd),
            //    EnableSsl = true
            //};
            //await smtpClient.SendMailAsync(message);
        }

        private (List<EmailAddress>, List<EmailAddress>, List<EmailAddress>) AddRecipients(IList<EmailRecipients> recipientsList)
        {
            var allEmails = new List<(EmailAddress Email, string Type)>();
            foreach (var recipients in recipientsList)
            {
                allEmails.AddRange(recipients.To?.Select(email => (new EmailAddress(email), "To")) ?? new List<(EmailAddress, string)>());
                allEmails.AddRange(recipients.CC?.Select(email => (new EmailAddress(email), "CC")) ?? new List<(EmailAddress, string)>());
                allEmails.AddRange(recipients.BCC?.Select(email => (new EmailAddress(email), "BCC")) ?? new List<(EmailAddress, string)>());
            }

            // Remove duplicates while preserving order
            var uniqueEmails = allEmails.GroupBy(x => x.Email.Email).Select(g => g.First()).ToList();

            var toEmails = uniqueEmails.Where(x => x.Type == "To").Select(x => x.Email).ToList();
            var ccEmails = uniqueEmails.Where(x => x.Type == "CC").Select(x => x.Email).ToList();
            var bccEmails = uniqueEmails.Where(x => x.Type == "BCC").Select(x => x.Email).ToList();
            // Distribute emails back to their respective lists
            return (toEmails, ccEmails, bccEmails);
        }

        private async Task<string> LoadTemplateFromFileAsync(string filePath, object[] model)
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory; // Adjust the base path as necessary
            var fullPath = Path.Combine(basePath, filePath);
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"Template file '{fullPath}' not found.");
            }

            var templateContent = await File.ReadAllTextAsync(fullPath);
            return ReplacePlaceholders(templateContent, model);
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

        private string ReplacePlaceholders(string template, object[] modelArgs)
        {
            foreach (var model in modelArgs)
            {
                var properties = model.GetType().GetProperties();
                foreach (var property in properties)
                {
                    var placeholder = $"{{{property.Name}}}";
                    var value = ConcatenateStrings(property.GetValue(model));
                    template = template.Replace(placeholder, value);
                }
            }
            return template;
        }
        private string ConcatenateStrings(object value)
        {
            if (value != null)
            {
                if (value is string[] || value is List<string>)
                {
                    return string.Join(", ", value as List<string>);
                }
                {
                    return value?.ToString() ?? string.Empty;
                }
            }
            return string.Empty;

        }
    }
}
