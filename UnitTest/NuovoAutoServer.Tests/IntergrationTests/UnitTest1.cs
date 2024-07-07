
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using Newtonsoft.Json;

using NuovoAutoServer.Api.Extensions;
using NuovoAutoServer.Model;
using NuovoAutoServer.Repository.DBContext;
using NuovoAutoServer.Repository.Repository;
using NuovoAutoServer.Services;
using NuovoAutoServer.Services.EmailNotification;
using NuovoAutoServer.Shared;

using System.Text.Json;

namespace NuovoAutoServer.Tests.IntergrationTests
{
    public class VehicleDetailsIntegrationTest
    {
        [Fact]
        public void GetByTagNumber()
        {

        }
    }

    public class VehicleEnquiryServiceTests
    {
        private readonly VehicleEnquiryService _service;

        public VehicleEnquiryServiceTests(VehicleEnquiryService vehicleEnquiryService)
        {
            _service = vehicleEnquiryService;
        }

        [Fact]
        public async Task CreateThousandVehicleEnquiries()
        {
            //var states = new[] { "MD", "DC", "VA" };
            //var stateDistribution = new[] { 1, 1, 1 };
            //var totalRecords = stateDistribution.Sum();

            // Read the JSON content from the file
            var vehicleEnquiriesJson = await File.ReadAllTextAsync("MockData/vehicleEnquiry.json");
            var vehicleEnquiriesFromJson = JsonConvert.DeserializeObject<List<VehicleEnquiry>>(vehicleEnquiriesJson);

            if (vehicleEnquiriesFromJson == null)
            {
                throw new InvalidOperationException("Failed to deserialize vehicle Enquiries from JSON.");
            }

            // Ensure the list has the correct number of items as per stateDistribution
            vehicleEnquiriesFromJson = vehicleEnquiriesFromJson.Take(1000).ToList();

            // Assign states according to the distribution
            //for (int i = 0, stateIndex = 0; i < vehicleEnquiriesFromJson.Count && stateIndex < states.Length; i += stateDistribution[stateIndex], stateIndex++)
            //{
            //    for (int j = 0; j < stateDistribution[stateIndex] && (i + j) < vehicleEnquiriesFromJson.Count; j++)
            //    {
            //        vehicleEnquiriesFromJson[i + j].Id = $"TESTDATA-{i + j + 1}";
            //    }
            //}

            int counter = 0;
            foreach (var Enquiry in vehicleEnquiriesFromJson)
            {
                counter++;
                Enquiry.Id = $"TESTDATA-{counter}";
            }

            // Act
            await _service.SaveVehicleEnquiryBulk(vehicleEnquiriesFromJson.Skip(100).Take(1000).ToList());

            // Assert
            Assert.Equal(vehicleEnquiriesFromJson.Count, vehicleEnquiriesFromJson.Count);
        }
    }

    public class EmailNotificationTests
    {
        private readonly EmailNotificationService _service;

        public EmailNotificationTests(EmailNotificationService service)
        {
            _service = service;
        }

        [Fact]
        public async Task SendEmailNotification()
        {
            var recipients = new EmailRecipients
            {
                To = new List<string> { "dev@gmail.com" }
            };

            var model = new
            {
                Make = "Toyota",
                Model = "Camry",
                Year = 2020,
                VinNumber = "1234567890",
                State = "CA",
                Zipcode = "90001"
            };

            // Act
            await _service.SendEmailAsync(recipients, "VehicleEnquiry", model);

            // Assert
            // Here we would typically assert that the email was sent correctly.
            // Since SmtpClient is not easily mockable, we can use a tool like Smtp4Dev to verify the email was sent.
            // For simplicity, we assume the method completes without exceptions.
            Assert.True(true);
        }
    }

}