using Microsoft.Extensions.Options;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using NuovoAutoServer.Model;
using NuovoAutoServer.Shared;

using Rest.ApiClient;
using Rest.ApiClient.Auth;

using System.Net.Http.Json;
using System.Numerics;

namespace NuovoAutoServer.Services.API_Provider
{
    public class VehicleDatabaseApiProvider : IVehicleDetailsApiProvider
    {
        private readonly IApiClient<VehicleDetails> _apiClient;
        private readonly AppSettings _appSettingsOptions;

        public VehicleDatabaseApiProvider(IApiClient<VehicleDetails> apiClient,  IOptions<AppSettings> appSettingsOptions)
        {
            _apiClient = apiClient;
            _appSettingsOptions = appSettingsOptions.Value;
        }

        //TODO: Configure the _appSettingsOptions.VehicleDatabasesApiProvider.BaseUrl while creating the IApiClient<VehicleDetails> object.
        public async Task<VehicleDetails?> GetByTagNumber(string tagNumber, string state)
        {
            var url = string.Format("{0}/license-decode/{1}/{2}", _appSettingsOptions.VehicleDatabasesApiProvider.BaseUrl, tagNumber, state);
           // url = string.Format("{0}/GetByTagNumber", _appSettingsOptions.VehicleDatabasesApiProvider.BaseUrl);

            var httpReq = new HttpRequestMessage(HttpMethod.Get, new Uri(url));
            var data = await _apiClient.SendAsync(httpReq);
            var res = await data.Content.ReadAsStringAsync();
            var jb = JObject.Parse(res);
            var vd = new VehicleDetails(tagNumber, jb["data"]["intro"]["vin"].ToString(), jb["data"] as JObject);
            vd.StateCode = state;
            return vd;
        }

        //TODO: Configure the _appSettingsOptions.VehicleDatabasesApiProvider.BaseUrl while creating the IApiClient<VehicleDetails> object.
        public async Task<VehicleDetails> GetByVinNumber(string vinNumber, string tagNumber = "")
        {
            var url = string.Format("{0}/vin-decode/{1}", _appSettingsOptions.VehicleDatabasesApiProvider.BaseUrl, vinNumber);
           // url = string.Format("{0}/GetByVinNumber", _appSettingsOptions.VehicleDatabasesApiProvider.BaseUrl);

            var httpReq = new HttpRequestMessage(HttpMethod.Get, new Uri(url));
            var data = await _apiClient.SendAsync(httpReq);
            var res = await data.Content.ReadAsStringAsync();
            var jb = JObject.Parse(res);
            var vd = new VehicleDetails(tagNumber, vinNumber, jb["data"] as JObject);
            vd.IsVinDetailsFetched = true;
            return vd;
        }
    }

    public class DummyApiProvider : IVehicleDetailsApiProvider
    {
        private readonly IApiClient<DummyApiProvider> _apiClient;
        public DummyApiProvider(IApiClient<DummyApiProvider> apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<VehicleDetails> GetByTagNumber(string tagNumber, string state)
        {
            var data = await this.GetByTagNumber();
            var jb = JObject.Parse(data);
            var vd = new VehicleDetails(tagNumber, jb["data"]["intro"]["vin"].ToString(), jb["data"] as JObject);
            vd.StateCode = state;
            await Task.CompletedTask;
            return vd;
        }

        public async Task<VehicleDetails> GetByVinNumber(string vinNumber, string tagNumber = "")
        {
            var data = this.GetByVinNumber();
            var jb = JObject.Parse(data);
            var vd = new VehicleDetails(tagNumber, vinNumber, jb["data"] as JObject);
            await Task.CompletedTask;
            return vd;
        }

        private async Task<string> GetByTagNumber()
        {
            var str = "{\n\"status\": \"success\",\n\"data\": {\n    \"intro\": {\n        \"vin\": \"1G4CW54K334142842\",\n        \"license\": \"CN31613\",\n        \"state\": \"IL\"\n    },\n    \"basic\": {\n        \"make\": \"Buick\",\n        \"model\": \"Park Avenue\",\n        \"year\": 2003\n    }\n  }\n}";

            //var httpReq = new HttpRequestMessage(HttpMethod.Get, new Uri("https://d4b24e62-24eb-47a6-ab77-7355e5d88517.mock.pstmn.io/GetByTagNumber"));
            //var data = await _apiClient.SendAsync(httpReq, AuthenticationKind.None);
            //var s = await data.Content.ReadAsStringAsync();

            return str;

        }

        private string GetByVinNumber()
        {
            var str = "{\r\n  \"status\": \"success\",\r\n  \"data\": {\r\n      \"intro\": {\r\n          \"vin\": \"5TDYK3DC8FS560664\"\r\n      },\r\n      \"basic\": {\r\n          \"make\": \"Toyota\",\r\n          \"model\": \"Sienna\",\r\n          \"year\": \"2015\",\r\n          \"trim\": \"\",\r\n          \"body_type\": \"4 Door Passenger Van\",\r\n          \"vehicle_type\": \"Van\",\r\n          \"vehicle_size\": \"\"\r\n      },\r\n      \"engine\": {\r\n          \"engine_size\": \"3.5\",\r\n          \"engine_description\": \"V6, 3.5L\",\r\n          \"engine_capacity\": \"\"\r\n      },\r\n      \"manufacturer\": {\r\n          \"manufacturer\": \"Toyota Motor Mfg., Indiana, Inc.\",\r\n          \"region\": \"Princeton, IN\",\r\n          \"country\": \"United States\",\r\n          \"plant_city\": \"Princeton, IN\"\r\n      },\r\n      \"transmission\": {\r\n          \"transmission_style\": \"\"\r\n      },\r\n      \"restraint\": {\r\n          \"others\": \"Dual Air Bag; Seat Belts; Side Air Bag-1st Row; Curtain Shield Air Bag-All Rows\"\r\n      },\r\n      \"dimensions\": {\r\n          \"gvwr\": \"Class D: 5,001-6,000Lbs\"\r\n      },\r\n      \"drivetrain\": {\r\n          \"drive_type\": \"Front Wheel Drive\"\r\n      },\r\n      \"fuel\": {\r\n          \"fuel_type\": \"Gasoline\"\r\n      }\r\n  }\r\n}";

            return str;
        }
    }


}
