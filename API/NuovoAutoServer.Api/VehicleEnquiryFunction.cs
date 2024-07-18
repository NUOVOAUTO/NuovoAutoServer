
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
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                await _vehicleEnquiryService.SaveVehicleEnquiry(vehicleEnquiry);

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(vehicleEnquiry);
                return response;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, ex.Message);
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                if (ex.InnerException is Microsoft.Azure.Cosmos.CosmosException cosmosEx)
                    await response.WriteStringAsync(cosmosEx.ResponseBody);
                else
                    await response.WriteStringAsync(ex.InnerException?.Message ?? ex.Message);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteStringAsync(ex.Message);
                return response;
            }
        }

        [Function("GetVehicleEnquiry")]
        public async Task<HttpResponseData> GetVehicleEnquiry([HttpTrigger(AuthorizationLevel.Function, "Get", Route = "VehicleEnquiry/{id}")] HttpRequestData req, Guid id)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var ve = await _vehicleEnquiryService.GetVehicleEnquiry(id);

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(ve);
                return response;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, ex.Message);
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                if (ex.InnerException is Microsoft.Azure.Cosmos.CosmosException cosmosEx)
                    await response.WriteStringAsync(cosmosEx.ResponseBody);
                else
                    await response.WriteStringAsync(ex.InnerException?.Message ?? ex.Message);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteStringAsync(ex.Message);
                return response;
            }
        }


        [Function("GetAllVehicleEnquiries")]
        public async Task<HttpResponseData> GetAllVehicleEnquiries([HttpTrigger(AuthorizationLevel.Function, "get", Route = "VehicleEnquiries")] HttpRequestData req)
        {
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

                var response = req.CreateResponse(HttpStatusCode.OK);
                //response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteAsJsonAsync(vehicleEnquiries.ToList());
                return response;
            }
            catch (Exception ex)
            {
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteStringAsync(ex.Message);
                return response;
            }
        }
    }
}
