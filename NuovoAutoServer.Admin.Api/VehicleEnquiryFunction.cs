using System.Net;
using System.Text;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using NuovoAutoServer.Model;
using NuovoAutoServer.Services;

namespace NuovoAutoServer.Admin.Api
{
    public class VehicleEnquiryFunction
    {
        private readonly ILogger<VehicleEnquiryFunction> _logger;
        private readonly VehicleEnquiryServiceSQL _vehicleEnquiryService;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public VehicleEnquiryFunction(ILogger<VehicleEnquiryFunction> logger, VehicleEnquiryServiceSQL vehicleEnquiryService, IOptions<JsonSerializerSettings> jsonSerializerSettings)
        {
            _logger = logger;
            _vehicleEnquiryService = vehicleEnquiryService;
            _jsonSerializerSettings = jsonSerializerSettings.Value;
        }

        [Function("UpdateVehicleEnquiryStatus")]
        public async Task<HttpResponseData> UpdateVehicleEnquiry([HttpTrigger(AuthorizationLevel.Function, "patch", Route = "UpdateVehicleEnquiryStatus")] HttpRequestData req, [FromBody] VehicleEnquiry vehicleEnquiry)
        {
            ApiResponseModel apiResponseModel = new(_jsonSerializerSettings);

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                await _vehicleEnquiryService.UpdateVehicleEnquiry(vehicleEnquiry);

                apiResponseModel.Data = vehicleEnquiry;
                apiResponseModel.IsSuccess = true;

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(apiResponseModel.ToJsonString());
                return response;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, ex.Message);

                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                if (ex.InnerException is Microsoft.Azure.Cosmos.CosmosException cosmosEx)
                {
                    _logger.LogError(cosmosEx.ResponseBody);
                }

                apiResponseModel.ErrorMessage = "Failed to update the details. Please verify the data.";
                await response.WriteStringAsync(apiResponseModel.ToJsonString());
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                apiResponseModel.ErrorMessage = ex.Message;
                await response.WriteStringAsync(apiResponseModel.ToJsonString());
                return response;
            }
        }

        [Function("GetVehicleEnquiry")]
        public async Task<HttpResponseData> GetVehicleEnquiry([HttpTrigger(AuthorizationLevel.Function, "Get", Route = "VehicleEnquiry/{id}")] HttpRequestData req, Guid id)
        {
            ApiResponseModel apiResponseModel = new(_jsonSerializerSettings);

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var ve = await _vehicleEnquiryService.GetVehicleEnquiry(id);

                apiResponseModel.Data = ve;
                apiResponseModel.IsSuccess = true;

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(apiResponseModel.ToJsonString());
                return response;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, ex.Message);

                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");

                if (ex.InnerException is Microsoft.Azure.Cosmos.CosmosException cosmosEx)
                    apiResponseModel.ErrorMessage = cosmosEx.ResponseBody;
                else
                    apiResponseModel.ErrorMessage = ex.InnerException?.Message ?? ex.Message;

                await response.WriteStringAsync(apiResponseModel.ToJsonString());
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                apiResponseModel.ErrorMessage = ex.Message;
                await response.WriteStringAsync(apiResponseModel.ToJsonString());
                return response;
            }
        }


        [Function("GetAllVehicleEnquiries")]
        public async Task<HttpResponseData> GetAllVehicleEnquiries([HttpTrigger(AuthorizationLevel.Function, "get", Route = "VehicleEnquiries")] HttpRequestData req)
        {
            ApiResponseModel apiResponseModel = new(_jsonSerializerSettings);
            try
            {
                int start = int.Parse(req.Query["start"] ?? "0");
                int end = int.Parse(req.Query["end"] ?? "25");
                bool fetchAll = bool.Parse(req.Query["all"] ?? false.ToString());

                int pageSize = 20;
                if (fetchAll)
                {
                    start = 0;
                    pageSize = 1000;
                }

                var (vehicleEnquiries, totalRecords) = await _vehicleEnquiryService.GetPaginatedAsync(start, pageSize);

                apiResponseModel.Data = vehicleEnquiries;
                apiResponseModel.IsSuccess = true;

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(apiResponseModel.ToJsonString());
                return response;
            }
            catch (Exception ex)
            {
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                apiResponseModel.ErrorMessage = ex.Message;
                await response.WriteStringAsync(apiResponseModel.ToJsonString());
                return response;
            }
        }
    }
}
