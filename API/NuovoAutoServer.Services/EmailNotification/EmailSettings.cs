namespace NuovoAutoServer.Services.EmailNotification
{
    public class EmailSettings
    {
        public string Sender { get; set; }
        public string SupportEmail { get; set; }
        public Dictionary<string, EmailTemplate> Templates { get; set; }
    }

    public class EmailTemplate
    {
        public string Subject { get; set; }
        public string Body { get; set; }

        public EmailRecipients Recipients { get; set; }
    }

    public class EmailRecipients
    {
        public List<string> To { get; set; }
        public List<string> CC { get; set; }
        public List<string> BCC { get; set; }
    }
}
