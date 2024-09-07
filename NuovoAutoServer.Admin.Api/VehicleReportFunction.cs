using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

using NuovoAutoServer.Services;
using NuovoAutoServer.Services.Services;

using System.Net;

namespace NuovoAutoServer.Admin.Api
{
    public class VehicleReportFunction
    {
        private readonly ILogger<VehicleReportFunction> _logger;
        private readonly BlobStorageService _blobStorageService;
        private readonly VehicleReportService _vehicleReportService;

        public VehicleReportFunction(ILogger<VehicleReportFunction> logger, BlobStorageService blobStorageService, VehicleReportService vehicleReportService)
        {
            _logger = logger;
            _blobStorageService = blobStorageService;
            _vehicleReportService = vehicleReportService;
        }

        /// <summary>
        /// TODO: Remove this once testing is done.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="vin"></param>
        /// <returns></returns>
        [Function("DownloadVinReportFromBlob")]
        public async Task<HttpResponseData> DownloadVinReportFromBlob([HttpTrigger(AuthorizationLevel.Function, "get", Route = "vin-auction-html/pdf/{vin}")] HttpRequestData req, string vin)
        {
            try
            {
                var blobContainer = "vin-reports-stash";
                var blobPath = $"{vin}.pdf";

                var blobContent = await _blobStorageService.GetBlobContentAsBytes(blobContainer, blobPath);

                if (blobContent == null)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    notFoundResponse.Headers.Add("Content-Type", "application/pdf");
                    return notFoundResponse;
                }

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/pdf");
                await response.WriteBytesAsync(blobContent);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while downloading the VIN report from blob.");
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                return errorResponse;
            }
        }

        [Function("DownloadVinReport")]
        public async Task<HttpResponseData> DownloadVinReport([HttpTrigger(AuthorizationLevel.Function, "get", Route = "DownloadVinReport/{vin}")] HttpRequestData req, string vin)
        {
            try
            {
                var vr = await _vehicleReportService.GetVinReport(vin);

                if (vr?.Content == null)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteStringAsync("File not found.");
                    return notFoundResponse;
                }

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/pdf");
                await response.WriteBytesAsync(vr.Content);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while downloading the VIN report.");
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await errorResponse.WriteStringAsync("An error occurred while downloading the VIN report.");
                return errorResponse;
            }
        }
    }
}
