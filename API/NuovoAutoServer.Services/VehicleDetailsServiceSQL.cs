using Microsoft.ApplicationInsights;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using NuovoAutoServer.Model;
using NuovoAutoServer.Repository.DBContext;
using NuovoAutoServer.Repository.Repository;
using NuovoAutoServer.Services.API_Provider;
using NuovoAutoServer.Shared;

using Polly;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuovoAutoServer.Services
{
    public class VehicleDetailsServiceSQL : IVehicleDetailsService
    {
        private readonly IVehicleDetailsApiProvider _vehicleDetailsApiProvider;
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger _logger;
        private readonly AppSettings _appSettings;
        private readonly RetryHandler _retryHandler;

        public VehicleDetailsServiceSQL(IVehicleDetailsApiProvider vehicleDetailsApiProvider, TelemetryClient telemetryClient, ILoggerFactory loggerFactory, IOptions<AppSettings> appSettings, RetryHandler retryHandler)
        {
            _vehicleDetailsApiProvider = vehicleDetailsApiProvider;
            _telemetryClient = telemetryClient;
            _logger = loggerFactory.CreateLogger<VehicleDetailsService>();
            _appSettings = appSettings.Value;
            _retryHandler = retryHandler;
        }

        private bool IsExpired(DateTimeOffset dt)
        {
            return dt.AddHours(_appSettings.CacheExpirationTimeInHours) <= TimeZoneInfo.ConvertTime(DateTimeOffset.Now, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
        }

        public async Task<VehicleDetails> GetByTagNumber(string tagNumber, string state)
        {
            VehicleDetails? vehicleDetails = await GetVinDetailsByTagFromDB(tagNumber);

            bool isExpired = vehicleDetails != null && IsExpired(vehicleDetails.LastUpdatedDateTime);
            _logger.LogInformation("Vehicle details expired: {IsExpired}", isExpired);

            if (isExpired || vehicleDetails == null)
            {
                var freshDetails = await _retryHandler.ExponentialRetry(async () => await _vehicleDetailsApiProvider.GetByTagNumber(tagNumber, state), "VehicleDetailsService.GetByTagNumber");

                if (freshDetails == null)
                {
                    string message = String.Format("Not able to fetch details from API for tag number: {0} and state: {1}", tagNumber, state);
                    _logger.LogError(message);
                    throw new Exception("Invalid Tagnumber or State");
                }

                var vinDetails = await GetVinDetailsByVinFromDB(freshDetails.Vin);
               
                if (vinDetails == null)
                {
                    VehicleDetails? freshVinDetails = null;
                    _logger.LogInformation("Fetching fresh VIN details for VIN: {Vin}", freshDetails.Vin);
                    freshVinDetails = await _retryHandler.ExponentialRetry(async () => await _vehicleDetailsApiProvider.GetByVinNumber(freshDetails.Vin, freshDetails.LicenseNumber), "VehicleDetailsService.GetByTagNumber.GetByVinNumber");
                    freshVinDetails.LicenseNumber = freshDetails.LicenseNumber;
                    freshVinDetails.StateCode = freshDetails.StateCode;
                    vehicleDetails = await AddOrUpdateVehicleDetailsAsync(freshVinDetails);
                }
                else
                {
                    vinDetails.LicenseNumber = freshDetails.LicenseNumber;
                    vinDetails.StateCode = freshDetails.StateCode;
                    await UpsertVinLicenseAssoication(vinDetails);
                    vehicleDetails = vinDetails;
                }
            }
            return vehicleDetails;
        }


        public async Task<VehicleDetails> GetByVinNumber(string vinNumber)
        {
            VehicleDetails? vehicleDetails = await GetVinDetailsByVinFromDB(vinNumber);

            bool isExpired = vehicleDetails != null && IsExpired(vehicleDetails.LastUpdatedDateTime);
            _logger.LogInformation("Vehicle details expired: {IsExpired}", isExpired);

            if (isExpired || vehicleDetails == null)
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
                vehicleDetails = await AddOrUpdateVehicleDetailsAsync(freshDetails);
            }

            return vehicleDetails;
        }


        private static async Task<VehicleDetails?> GetVinDetailsByTagFromDB(string tagNumber)
        {
            string jsonResult;

            Dictionary<string, object> parameters = new Dictionary<string, object>()
                {
                    {"@LicenseNumber", tagNumber}
                };

            // Execute the stored procedure
            string commandText = "GET_VINDETAILS";
            jsonResult = await SqlHelper.ExecuteStoreProcedureAsync(commandText, parameters);

            var vd = JsonConvert.DeserializeObject<VehicleDetails>(jsonResult);
            return vd;
        }

        private static async Task<VehicleDetails?> GetVinDetailsByVinFromDB(string vin)
        {
            string jsonResult;

            Dictionary<string, object> parameters = new Dictionary<string, object>()
                {
                    {"@Vin", vin}
                };

            // Execute the stored procedure
            string commandText = "GET_VINDETAILS";
            jsonResult = await SqlHelper.ExecuteStoreProcedureAsync(commandText, parameters);
            var vd = JsonConvert.DeserializeObject<VehicleDetails>(jsonResult);
            return vd;
        }

        private async Task<VehicleDetails> AddOrUpdateVehicleDetailsAsync(VehicleDetails freshVinDetails)
        {
            string jsonResult;

            Dictionary<string, object> parameters = new Dictionary<string, object>()
                {
                    {"@Vin", freshVinDetails.Vin},
                    {"@LicenseNumber", freshVinDetails.LicenseNumber},
                    {"@StateCode", freshVinDetails.StateCode},
                    {"@Make", freshVinDetails.Make},
                    {"@Model", freshVinDetails.Model},
                    {"@Year", freshVinDetails.Year},
                    {"@Drivetrain", freshVinDetails.Drivetrain},
                    {"@Transmission", freshVinDetails.Transmission},
                    {"@OtherDetails", JsonConvert.SerializeObject(freshVinDetails.BasicDetails)},
                };

            // Execute the stored procedure
            string commandText = "UPSERT_VINDETAILS_ASSOCIATION";
            jsonResult = await SqlHelper.ExecuteStoreProcedureAsync(commandText, parameters);

            var vd = JsonConvert.DeserializeObject<VehicleDetails>(jsonResult);
            return vd;
        }

        private async Task UpsertVinLicenseAssoication(VehicleDetails freshVinDetails)
        {
            string jsonResult;

            Dictionary<string, object> parameters = new Dictionary<string, object>()
                {
                    {"@Vin", freshVinDetails.Vin},
                    {"@LicenseNumber", freshVinDetails.LicenseNumber},
                    {"@StateCode", freshVinDetails.StateCode},
                };

            // Execute the stored procedure
            string commandText = "[UPSERT_VIN_LICENSE_ASSOCIATION]";
            await SqlHelper.ExecuteStoreProcedureAsync(commandText, parameters);
        }
    }
}
