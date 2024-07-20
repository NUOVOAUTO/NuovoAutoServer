using System.Net;
using System.Text;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using NuovoAutoServer.Api.Extensions;
using NuovoAutoServer.Model;
using NuovoAutoServer.Services;
using NuovoAutoServer.Shared;
using NuovoAutoServer.Shared.CustomExceptions;

namespace NuovoAutoServer.Api
{
    public class VehicleDetailsFunction
    {
        private readonly ILogger _logger;

        private readonly VehicleDetailsServiceSQL _vehicleDetailsService;
        private readonly SecurityService _securityService; // Added SecurityService
        private readonly RetryHandler _retryHandler; // Added RetryHandler

        public VehicleDetailsFunction(ILoggerFactory loggerFactory, VehicleDetailsServiceSQL vehicleDetialsService, SecurityService securityService, RetryHandler retryHandler) // Injected SecurityService and RetryHandler
        {
            _logger = loggerFactory.CreateLogger<VehicleDetailsFunction>();
            _vehicleDetailsService = vehicleDetialsService;
            _securityService = securityService; // Assigned injected SecurityService
            _retryHandler = retryHandler; // Assigned injected RetryHandler
        }

        [Function("Function1")]
        public async Task<HttpResponseData> Function1([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                var response = req.CreateResponse(HttpStatusCode.ExpectationFailed);
                await response.WriteStringAsync(JsonConvert.SerializeObject(new { }));

                return response;
            }
            catch (Exception ex)
            {
                var response = req.CreateResponse(HttpStatusCode.ExpectationFailed);
                await response.WriteStringAsync(JsonConvert.SerializeObject(new { ErrorMessage = ex.Message }));

                return response;
            }
        }

        [Function("GetByTagNumber")]
        public async Task<HttpResponseData> GetByTagNumber([HttpTrigger(AuthorizationLevel.Function, "get",
        Route = "VehicleDetails/searchByTagNumber/{tag}/{state}")] HttpRequestData req, string tag, string state,
FunctionContext executionContext)
        {
            ApiResponseModel apiResponseModel = new();
            try
            {
                _securityService.ValidateClientIp(req);

                _logger.LogInformation("C# HTTP trigger function processed a request.");

                var vd = await _vehicleDetailsService.GetByTagNumber(tag.Trim(), state.Trim());

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");

                apiResponseModel.Data = vd;
                apiResponseModel.IsSuccess = true;
                await response.WriteStringAsync(JsonConvert.SerializeObject(apiResponseModel));
                return response;
            }
            catch (IPNotFoundException ex)
            {
                var response = req.CreateResponse(HttpStatusCode.Forbidden);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                _logger.LogError(ex.Message);
                //response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                apiResponseModel.ErrorMessage = ex.Message;
                await response.WriteStringAsync(JsonConvert.SerializeObject(apiResponseModel));
                return response;
            }
            catch (RateLimitExceededException ex)
            {
                var response = req.CreateResponse(HttpStatusCode.TooManyRequests);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                _logger.LogError(ex.Message);
                apiResponseModel.ErrorMessage = ex.Message;
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

        [Function("GetByVinNumber")]
        public async Task<HttpResponseData> GetByVinNumber([HttpTrigger(AuthorizationLevel.Function, "get",
        Route = "VehicleDetails/searchByVinNumber/{vin}")] HttpRequestData req, string vin,
FunctionContext executionContext)
        {
            ApiResponseModel apiResponseModel = new ();
            try
            {
                _securityService.ValidateClientIp(req);

                _logger.LogInformation("C# HTTP trigger function processed a request.");

                var vd = await _vehicleDetailsService.GetByVinNumber(vin.Trim());

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");

                apiResponseModel.Data = vd;
                apiResponseModel.IsSuccess = true;
                await response.WriteStringAsync(JsonConvert.SerializeObject(apiResponseModel));
                return response;
            }
            catch (IPNotFoundException ex)
            {
                var response = req.CreateResponse(HttpStatusCode.Forbidden);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                _logger.LogError(ex.Message);
                apiResponseModel.ErrorMessage = ex.Message;
                await response.WriteStringAsync(JsonConvert.SerializeObject(apiResponseModel));
                return response;
            }
            catch (RateLimitExceededException ex)
            {
                var response = req.CreateResponse(HttpStatusCode.TooManyRequests);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                _logger.LogError(ex.Message);
                apiResponseModel.ErrorMessage = ex.Message;
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
