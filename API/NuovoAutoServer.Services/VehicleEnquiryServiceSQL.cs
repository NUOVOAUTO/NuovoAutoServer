using Microsoft.ApplicationInsights;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
using System.Threading.Tasks;

namespace NuovoAutoServer.Services
{
    public class VehicleEnquiryServiceSQL : IVehicleEnquiryService
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger _logger;
        private readonly AppSettings _appSettings;
        private readonly EmailNotificationService _emailNotificationService;

        public VehicleEnquiryServiceSQL(TelemetryClient telemetryClient, ILoggerFactory loggerFactory, IOptions<AppSettings> appSettings, EmailNotificationService emailNotificationService)
        {
            _telemetryClient = telemetryClient;
            _logger = loggerFactory.CreateLogger<VehicleDetailsService>();
            _appSettings = appSettings.Value;
            _emailNotificationService = emailNotificationService;
        }

        public Task<(IEnumerable<VehicleEnquiry> Items, int TotalCount)> GetPaginatedAsync(int start, int pageSize)
        {
            throw new NotImplementedException();
        }

        public async Task SaveVehicleEnquiry(VehicleEnquiry vehicleEnquiry)
        {
            using (var context = new SqlDbContext())
            {
                if (vehicleEnquiry.Id == null)
                {
                    await context.VehicleEnquiry.AddAsync(vehicleEnquiry);
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
