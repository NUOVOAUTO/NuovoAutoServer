using System.Net;
using System.Text;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using NuovoAutoServer.Model;
using NuovoAutoServer.Services;

namespace NuovoAutoServer.Api
{
    public class VehicleDetailsFunction
    {
        private readonly ILogger _logger;

        private readonly VehicleDetailsService _vehicleDetailsService;
        public VehicleDetailsFunction(ILoggerFactory loggerFactory, VehicleDetailsService vehicleDetialsService)
        {
            _logger = loggerFactory.CreateLogger<VehicleDetailsFunction>();
            _vehicleDetailsService = vehicleDetialsService;
        }

        [Function("Function1")]
        public async Task<HttpResponseData> Function1([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new { });

            return response;
        }

        [Function("GetByTagNumber")]
        public async Task<HttpResponseData> GetByTagNumber([HttpTrigger(AuthorizationLevel.Function, "get",
Route = "VehicleDetails/searchByTagNumber/{tag}/{state}")] HttpRequestData req, string tag, string state,
FunctionContext executionContext)
        {
            try
            {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var vd = await _vehicleDetailsService.GetByTagNumber(tag, state);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");

            var json = JsonConvert.SerializeObject(vd);
            await response.WriteStringAsync(json);
            return response;
            }
            catch (Exception ex)
            {
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(ex.Message);
                return response;
            }
        }

        [Function("GetByVinNumber")]
        public async Task<HttpResponseData> GetByVinNumber([HttpTrigger(AuthorizationLevel.Function, "get",
Route = "VehicleDetails/searchByVinNumber/{vin}")] HttpRequestData req, string vin,
FunctionContext executionContext)
        {
            try
            {
                _logger.LogInformation("C# HTTP trigger function processed a request.");

                var vd = await _vehicleDetailsService.GetByVinNumber(vin);

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");

                var json = JsonConvert.SerializeObject(vd);
                await response.WriteStringAsync(json);
                return response;
            }
            catch (Exception ex)
            {
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(ex.Message);
                return response;
            }
        }
    }
}
