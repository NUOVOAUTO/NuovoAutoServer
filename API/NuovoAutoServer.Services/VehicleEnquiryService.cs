using Azure;

using Microsoft.ApplicationInsights;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Abstractions;

using Newtonsoft.Json;

using NuovoAutoServer.Model;
using NuovoAutoServer.Repository.DBContext;
using NuovoAutoServer.Repository.Repository;
using NuovoAutoServer.Services.API_Provider;
using NuovoAutoServer.Services.EmailNotification;
using NuovoAutoServer.Shared;

using Polly;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace NuovoAutoServer.Services
{
    [Obsolete]
    public class VehicleEnquiryService : IVehicleEnquiryService
    {
        private readonly IGenericRepository<CosmosDBContext> _repo;
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger _logger;
        private readonly AppSettings _appSettings;
        private readonly EmailNotificationService _emailNotificationService;

        public VehicleEnquiryService(IGenericRepository<CosmosDBContext> repository, TelemetryClient telemetryClient, ILoggerFactory loggerFactory, IOptions<AppSettings> appSettings, EmailNotificationService emailNotificationService)
        {
            _repo = repository;
            _telemetryClient = telemetryClient;
            _logger = loggerFactory.CreateLogger<VehicleDetailsService>();
            _appSettings = appSettings.Value;
            _emailNotificationService = emailNotificationService;
        }

        public async Task SaveVehicleEnquiry(VehicleEnquiry vehicleEnquiry)
        {
            var vr = vehicleEnquiry.Validate();
            if (vr.Any())
            {
                throw new ValidationException(JsonConvert.SerializeObject(new { errors = vr.Select(x => x.ErrorMessage) }));
            }

            vehicleEnquiry.SetPartitionKey();
            vehicleEnquiry.OnCreated();
            vehicleEnquiry.SubmittedOn = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));

            await _repo.AddAsync(vehicleEnquiry);
            try
            {
                await SendEmail(vehicleEnquiry);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to send email: SendEmailAsync");
                _logger.LogError(ex, ex.Message);
            }
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
                vehicleEnquiry);
        }

        public async Task SaveVehicleEnquiryBulk(List<VehicleEnquiry> vehicleEnquiries)
        {
            foreach (var vehicleEnquiry in vehicleEnquiries)
            {
                vehicleEnquiry.SetPartitionKey();
                vehicleEnquiry.OnCreated();
            }
            await _repo.AddRangeAsync(vehicleEnquiries);
        }

        public async Task<(IEnumerable<VehicleEnquiry> Items, int TotalCount)> GetPaginatedAsync(int start, int pageSize)
        {
            var query = _repo.Get<VehicleEnquiry>();
            //var totalCount = await query.CountAsync();
            var totalCount = -1;
            var items = await query.OrderBy(v => v.LastUpdatedDateTime)
                                      .Skip(start)
                                      .Take(pageSize)
                                      .ToListAsync();

            return (items, totalCount);
        }

        public Task<VehicleEnquiry> GetVehicleEnquiry(Guid id)
        {
            throw new NotImplementedException();
        }
    }


}
