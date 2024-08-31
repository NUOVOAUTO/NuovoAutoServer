
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using NuovoAutoServer.Model;
using NuovoAutoServer.Services;

using System.Net;

namespace NuovoAutoServer.Api
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

        [Function("SaveVehicleEnquiry")]
        public async Task<HttpResponseData> SaveVehicleEnquiry([HttpTrigger(AuthorizationLevel.Function, "post", Route = "SaveVehicleEnquiry")] HttpRequestData req, [FromBody] VehicleEnquiry vehicleEnquiry)
        {
            ApiResponseModel apiResponseModel = new(_jsonSerializerSettings);
           
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                await _vehicleEnquiryService.SaveVehicleEnquiry(vehicleEnquiry);

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
                apiResponseModel.ErrorMessage = ex.Message;
                apiResponseModel.IsSuccess = true;
                await response.WriteStringAsync(apiResponseModel.ToJsonString());
                return response;
            }
        }

      
    }
}
