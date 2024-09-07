using Microsoft.Extensions.Options;

using Newtonsoft.Json.Linq;

using NuovoAutoServer.Model;
using NuovoAutoServer.Shared;

using Rest.ApiClient;

using System.Reflection.Metadata;

namespace NuovoAutoServer.Services.API_Provider
{
    public class VehicleDatabasesReportApiProvider : IVehicleReportApiProvider
    {

        private readonly IApiClient<VehicleReport> _apiClient;
        private readonly AppSettings _appSettingsOptions;

        public VehicleDatabasesReportApiProvider(IApiClient<VehicleReport> apiClient, IOptions<AppSettings> appSettingsOptions)
        {
            _apiClient = apiClient;
            _appSettingsOptions = appSettingsOptions.Value;
        }
        public async Task<VehicleReport> DownloadVinReport(string vin)
        {
            var url = string.Format("{0}/vin-auction-html/pdf/{1}", _appSettingsOptions.VehicleDatabasesReportApiProvider.BaseUrl, vin);
            // url = string.Format("{0}/GetByTagNumber", _appSettingsOptions.VehicleDatabasesApiProvider.BaseUrl);
            var httpReq = new HttpRequestMessage(HttpMethod.Get, new Uri(url));
            var data = await _apiClient.SendAsync(httpReq);
            byte[] res = await data.Content.ReadAsByteArrayAsync();
            var vr = new VehicleReport(vin, res);
            return vr;
        }
    }
}
