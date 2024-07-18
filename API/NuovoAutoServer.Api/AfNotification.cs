using System;
using System.Threading.Tasks;

using Azure.Messaging.ServiceBus;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using NuovoAutoServer.Model;
using NuovoAutoServer.Services.EmailNotification;
using NuovoAutoServer.Shared;

namespace NuovoAutoServer.Api
{
    public class AfNotification
    {
        private readonly ILogger<AfNotification> _logger;
        private readonly EmailNotificationService _emailNotificationService;

        public AfNotification(ILogger<AfNotification> logger, EmailNotificationService emailNotificationService)
        {
            _logger = logger;
            _emailNotificationService = emailNotificationService;
        }

        [Function(nameof(AfNotification))]
        public async Task Run(
            [ServiceBusTrigger(AzureServiceBusClient._queueName, Connection = "ServiceBusConnection")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            _logger.LogInformation("Message ID: {id}", message.MessageId);

            var queueInput = JsonConvert.DeserializeObject<VehicleEnquiry>(message.Body.ToString());

            if (queueInput == null)
            {
                _logger.LogError($"{AzureServiceBusClient._queueName} queueInput is null for messageId: {message.MessageId} ");
            }
            else
            {
                await SendEmail(queueInput);
            }
            // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }

        private async Task SendEmail(VehicleEnquiry vehicleEnquiry)
        {
            var recipients = new EmailRecipients
            {
                To = new List<string> { vehicleEnquiry.Email }
            };
            await _emailNotificationService.SendEmailAsync(
                recipients,
                "VehicleEnquiry",
                new object[] { vehicleEnquiry, vehicleEnquiry.VehicleEnquiryDetails });
        }
    }
}
