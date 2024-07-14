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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace NuovoAutoServer.Services
{
    [Obsolete]
    public class VehicleDetailsService : IVehicleDetailsService
    {
        private readonly IGenericRepository<CosmosDBContext> _repo;
        private readonly IVehicleDetailsApiProvider _vehicleDetailsApiProvider;
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger _logger;
        private readonly AppSettings _appSettings;
        private readonly RetryHandler _retryHandler;

        private bool IsExpired(DateTimeOffset dt)
        {
            return dt.AddHours(_appSettings.CacheExpirationTimeInHours) <= TimeZoneInfo.ConvertTime(DateTimeOffset.Now, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
        }

        public VehicleDetailsService(IGenericRepository<CosmosDBContext> repository, IVehicleDetailsApiProvider vehicleDetailsApiProvider, TelemetryClient telemetryClient, ILoggerFactory loggerFactory, IOptions<AppSettings> appSettings, RetryHandler retryHandler)
        {
            _repo = repository;
            _vehicleDetailsApiProvider = vehicleDetailsApiProvider;
            _telemetryClient = telemetryClient;
            _logger = loggerFactory.CreateLogger<VehicleDetailsService>();
            _appSettings = appSettings.Value;
            _retryHandler = retryHandler;
        }

        public async Task<VehicleDetails> GetByTagNumber(string tagNumber, string state)
        {
            // Query to get the vehicle details by tag number and state
            var vehicleDetails = await _repo.Get<VehicleDetails>()
                                            .Where(x => x.PartitionKey.StartsWith(tagNumber) && x.StateCode == state)
                                            .FirstOrDefaultAsync();

            // Check if the vehicle details are expired
            bool isExpired = vehicleDetails != null && IsExpired(vehicleDetails.LastUpdatedDateTime);
            _logger.LogInformation("Vehicle details expired: {IsExpired}", isExpired);

            // If the vehicle details are expired or not found, get fresh details from API
            if (isExpired || vehicleDetails == null)
            {
                _logger.LogInformation("Fetching fresh details from API for tag number: {0} and state: {1}", tagNumber, state);
                var freshDetails = await _retryHandler.ExponentialRetry(async () => await _vehicleDetailsApiProvider.GetByTagNumber(tagNumber, state), "VehicleDetailsService.GetByTagNumber");

                if (freshDetails == null)
                {
                    string message = String.Format("Not able to fetch details from API for tag number: {0} and state: {1}", tagNumber, state);
                    _logger.LogWarning(message);
                    throw new Exception("Invalid Tagnumber or State");
                }

                var vinDetails = await _repo.Get<VehicleDetails>()
                                          .Where(x => x.PartitionKey.Contains(freshDetails.Vin) && x.Vin == freshDetails.Vin)
                                          .FirstOrDefaultAsync();

                if (vinDetails != null)
                {
                    await this._repo.RemoveAsync(vinDetails);
                }

                VehicleDetails? freshVinDetails = null;
                _logger.LogInformation("Fetching fresh VIN details for VIN: {Vin}", freshDetails.Vin);
                freshVinDetails = await _retryHandler.ExponentialRetry(async () => await _vehicleDetailsApiProvider.GetByVinNumber(freshDetails.Vin, freshDetails.LicenseNumber), "VehicleDetailsService.GetByTagNumber.GetByVinNumber");
                freshVinDetails.LicenseNumber = freshDetails.LicenseNumber;
                freshVinDetails.StateCode = freshDetails.StateCode;
                vehicleDetails = await AddOrUpdateVehicleDetailsAsync(null, freshVinDetails);
            }

            return vehicleDetails;
        }

        public async Task<VehicleDetails> GetByVinNumber(string vinNumber)
        {
            var vehicleDetails = await _repo.Get<VehicleDetails>()
                                            .Where(x => x.PartitionKey.Contains(vinNumber) && x.Vin == vinNumber)
                                            .FirstOrDefaultAsync();

            // Check if the vehicle details are expired
            bool isExpired = vehicleDetails != null && IsExpired(vehicleDetails.LastUpdatedDateTime);
            _logger.LogInformation("Vehicle details expired: {IsExpired}", isExpired);

            // If the vehicle details are expired or not found, get fresh details from API
            if (isExpired || vehicleDetails == null || vehicleDetails?.IsVinDetailsFetched == false)
            {
                _logger.LogInformation("Fetching fresh details from API for VIN: {0}", vinNumber);
                var freshDetails = await _retryHandler.ExponentialRetry(async () => await _vehicleDetailsApiProvider.GetByVinNumber(vinNumber), "VehicleDetailsService.GetByVinNumber");

                if (freshDetails == null)
                {
                    string message = String.Format("Not able to fetch details from API for VIN: {0}", vinNumber);
                    _logger.LogWarning(message);
                    throw new Exception("Invalid VIN number");
                }

                // If vehicle details already exist in the repository, update it
                vehicleDetails = await AddOrUpdateVehicleDetailsAsync(vehicleDetails, freshDetails);
            }

            return vehicleDetails;
        }

        private async Task<VehicleDetails?> AddOrUpdateVehicleDetailsAsync(VehicleDetails? vehicleDetails, VehicleDetails freshDetails)
        {
            if (vehicleDetails != null)
            {
                vehicleDetails.SetEntity(freshDetails);
                vehicleDetails = await _repo.UpdateAsync(vehicleDetails);
            }
            else // If vehicle details do not exist in the repository, add new details
            {
                freshDetails.Id = Guid.NewGuid().ToString();
                vehicleDetails = await _repo.AddAsync(freshDetails);
            }

            return vehicleDetails;
        }
    }


}
