using Azure;

using Microsoft.ApplicationInsights;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Abstractions;

using NuovoAutoServer.Model;
using NuovoAutoServer.Repository.DBContext;
using NuovoAutoServer.Repository.Repository;
using NuovoAutoServer.Services.API_Provider;
using NuovoAutoServer.Shared;

using Polly;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace NuovoAutoServer.Services
{
    public class VehicleEnquiryService
    {
        private readonly IGenericRepository<CosmosDBContext> _repo;
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger _logger;
        private readonly AppSettings _appSettings;

        public VehicleEnquiryService(IGenericRepository<CosmosDBContext> repository,TelemetryClient telemetryClient, ILoggerFactory loggerFactory, IOptions<AppSettings> appSettings)
        {
            _repo = repository;
            _telemetryClient = telemetryClient;
            _logger = loggerFactory.CreateLogger<VehicleDetailsService>();
            _appSettings = appSettings.Value;
        }

        public async Task SaveVehicleEnquiry(VehicleEnquiry vehicleEnquiry)
        {
            vehicleEnquiry.SetPartitionKey();
            vehicleEnquiry.OnCreated();
            vehicleEnquiry.SubmittedOn = DateTime.UtcNow;

            await _repo.AddAsync(vehicleEnquiry);
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
    }


}
