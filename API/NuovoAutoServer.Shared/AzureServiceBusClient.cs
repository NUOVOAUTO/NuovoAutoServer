using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuovoAutoServer.Shared
{
    public class AzureServiceBusClient
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusAdministrationClient _adminClient;
        public const string _queueName = "email-notification";

        public AzureServiceBusClient(ServiceBusClient client, ServiceBusAdministrationClient adminClient)
        {
            _client = client;
            _adminClient = adminClient;
        }

        public async Task InitializeQueueAsync()
        {
            if (!await _adminClient.QueueExistsAsync(_queueName))
            {
                var options = new CreateQueueOptions(_queueName)
                {
                   LockDuration = TimeSpan.FromMinutes(5),
                   DeadLetteringOnMessageExpiration = true,
                   MaxDeliveryCount = 3
                };
                await _adminClient.CreateQueueAsync(options);
            }
        }

        public async Task SendMessageAsync(string messageBody)
        {
            var sender = _client.CreateSender(_queueName);
            var message = new ServiceBusMessage(messageBody);
            await sender.SendMessageAsync(message);
        }
    }
}
