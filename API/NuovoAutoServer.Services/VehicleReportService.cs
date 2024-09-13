using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NuovoAutoServer.Model;
using NuovoAutoServer.Services.API_Provider;
using NuovoAutoServer.Services.Services;
using NuovoAutoServer.Shared;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuovoAutoServer.Services
{
    public class VehicleReportService
    {
        private readonly ILogger _logger;
        private readonly IVehicleReportApiProvider _vehicleReportApiProvider;
        private readonly RetryHandler _retryHandler;
        private readonly BlobStorageService _blobStorageService;

        public VehicleReportService(IVehicleReportApiProvider vehicleReportApiProvider, TelemetryClient telemetryClient, ILoggerFactory loggerFactory, IOptions<AppSettings> appSettings, RetryHandler retryHandler, BlobStorageService blobStorageService)
        {
            _vehicleReportApiProvider = vehicleReportApiProvider;
            _logger = loggerFactory.CreateLogger<VehicleReportService>();
            _retryHandler = retryHandler;
            _blobStorageService = blobStorageService;
        }

        public async Task<VehicleReport?> GetVinReport(string vin)
        {
            var blobContainer = "vin-reports";
            var blobPath = $"{vin}.pdf";

            _logger.LogInformation($"Getting vehicle report for VIN: {vin}");

            var blobContent = await _blobStorageService.GetBlobContentAsBytes(blobContainer, blobPath);

            if (blobContent != null)
            {
                _logger.LogInformation($"Vehicle report found in blob storage for VIN: {vin}");
                return new VehicleReport
                {
                    Vin = vin,
                    Content = blobContent
                };
            }

            _logger.LogInformation($"Vehicle report not found in blob storage for VIN: {vin}. Downloading from API.");
            var vehicleReport = await _vehicleReportApiProvider.DownloadVinReport(vin);

            if (vehicleReport?.Content != null)
            {
                _logger.LogInformation($"Uploading vehicle report to blob storage for VIN: {vin}");
                await _blobStorageService.UploadToBlob(blobContainer, blobPath, vehicleReport.Content);
            }

            return vehicleReport;
        }
    }
}
