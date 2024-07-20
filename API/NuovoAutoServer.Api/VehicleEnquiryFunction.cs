
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

        public VehicleEnquiryFunction(ILogger<VehicleEnquiryFunction> logger, VehicleEnquiryServiceSQL vehicleEnquiryService)
        {
            _logger = logger;
            _vehicleEnquiryService = vehicleEnquiryService;
        }

        [Function("SaveVehicleEnquiry")]
        public async Task<HttpResponseData> SaveVehicleEnquiry([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req, [FromBody] VehicleEnquiry vehicleEnquiry)
        {
            ApiResponseModel apiResponseModel = new();
           
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                await _vehicleEnquiryService.SaveVehicleEnquiry(vehicleEnquiry);

                apiResponseModel.Data = vehicleEnquiry;
                apiResponseModel.IsSuccess = true;

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonConvert.SerializeObject(apiResponseModel));
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

                await response.WriteStringAsync(JsonConvert.SerializeObject(apiResponseModel));
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                apiResponseModel.ErrorMessage = ex.Message;
                apiResponseModel.IsSuccess = true;
                await response.WriteStringAsync(JsonConvert.SerializeObject(apiResponseModel));
                return response;
            }
        }

        [Function("GetVehicleEnquiry")]
        public async Task<HttpResponseData> GetVehicleEnquiry([HttpTrigger(AuthorizationLevel.Function, "Get", Route = "VehicleEnquiry/{id}")] HttpRequestData req, Guid id)
        {
            ApiResponseModel apiResponseModel = new();

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var ve = await _vehicleEnquiryService.GetVehicleEnquiry(id);

                apiResponseModel.Data = ve;
                apiResponseModel.IsSuccess = true;

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonConvert.SerializeObject(apiResponseModel));
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

                await response.WriteStringAsync(JsonConvert.SerializeObject(apiResponseModel));
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                apiResponseModel.ErrorMessage = ex.Message;
                await response.WriteStringAsync(JsonConvert.SerializeObject(apiResponseModel));
                return response;
            }
        }


        [Function("GetAllVehicleEnquiries")]
        public async Task<HttpResponseData> GetAllVehicleEnquiries([HttpTrigger(AuthorizationLevel.Function, "get", Route = "VehicleEnquiries")] HttpRequestData req)
        {
            ApiResponseModel apiResponseModel = new();
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
                await response.WriteStringAsync(JsonConvert.SerializeObject(apiResponseModel));
                return response;
            }
            catch (Exception ex)
            {
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                apiResponseModel.ErrorMessage = ex.Message;
                await response.WriteStringAsync(JsonConvert.SerializeObject(apiResponseModel));
                return response;
            }
        }
    }
}
