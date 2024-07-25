using Microsoft.ApplicationInsights;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using NuovoAutoServer.Model;
using NuovoAutoServer.Repository.DBContext;
using NuovoAutoServer.Repository.Repository;
using NuovoAutoServer.Services.EmailNotification;
using NuovoAutoServer.Shared;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NuovoAutoServer.Services
{
    public class VehicleEnquiryServiceSQL : IVehicleEnquiryService
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger _logger;
        private readonly AppSettings _appSettings;
        private readonly EmailNotificationService _emailNotificationService;
        private readonly AzureServiceBusClient _serviceBusClient;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public VehicleEnquiryServiceSQL(TelemetryClient telemetryClient, ILoggerFactory loggerFactory, IOptions<AppSettings> appSettings, EmailNotificationService emailNotificationService, AzureServiceBusClient serviceBusClient, IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _telemetryClient = telemetryClient;
            _logger = loggerFactory.CreateLogger<VehicleDetailsService>();
            _appSettings = appSettings.Value;
            _emailNotificationService = emailNotificationService;
            _serviceBusClient = serviceBusClient;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
        }

        public async Task<(IEnumerable<VehicleEnquiry> Items, int TotalCount)> GetPaginatedAsync(int start, int pageSize)
        {
            var totalCount = -1;
            IEnumerable<VehicleEnquiry> items = null;
            using (var context = new SqlDbContext())
            {
                var query = context.VehicleEnquiry.Include(x => x.VehicleEnquiryDetails).AsQueryable().AsSplitQuery();
                //var totalCount = await query.CountAsync();
               
                items = await query.OrderByDescending(v => v.SubmittedOn)
                                          .Skip(start)
                                          .Take(pageSize)
                                          .ToListAsync();
            }
            return (items, totalCount);
        }

        public async Task SaveVehicleEnquiry(VehicleEnquiry vehicleEnquiry)
        {
            using (var context = new SqlDbContext())
            {
                if (vehicleEnquiry.Id == null)
                {
                    await context.VehicleEnquiry.AddAsync(vehicleEnquiry);

                    try
                    {
                        var messageBody = System.Text.Json.JsonSerializer.Serialize(vehicleEnquiry, _jsonSerializerOptions);
                        await _serviceBusClient.SendMessageAsync(messageBody);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Failed to send email: SendEmailAsync");
                        _logger.LogError(ex, ex.Message);
                    }
                }
                else
                {
                    vehicleEnquiry.OnChanged();
                    context.VehicleEnquiry.Update(vehicleEnquiry);
                }

                //save data to the database tables
                await context.SaveChangesAsync();
            }
        }

        public async Task UpdateVehicleEnquiry(VehicleEnquiry vehicleEnquiry)
        {
            using (var context = new SqlDbContext())
            {
                var existingEnquiry = await context.VehicleEnquiry.FindAsync(vehicleEnquiry.Id);
                if (existingEnquiry != null)
                {
                    existingEnquiry.OnChanged();

                    existingEnquiry.EnquiryStatus = vehicleEnquiry.EnquiryStatus;
                    existingEnquiry.EnquiryComments = vehicleEnquiry.EnquiryComments;
                    context.VehicleEnquiry.Update(existingEnquiry);
                    await context.SaveChangesAsync();
                }
            }
        }

        public async Task<VehicleEnquiry> GetVehicleEnquiry(Guid id)
        {
            VehicleEnquiry vehicleEnquiry;
            using (var context = new SqlDbContext())
            {
                //add entitiy to the context
                vehicleEnquiry = await context.VehicleEnquiry.Where(x => x.Id == id).Include(x => x.VehicleEnquiryDetails).FirstOrDefaultAsync();
            }
            return vehicleEnquiry;
        }

        public Task SaveVehicleEnquiryBulk(List<VehicleEnquiry> vehicleEnquiries)
        {
            throw new NotImplementedException();
        }
    }
}
