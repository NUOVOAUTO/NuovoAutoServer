using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuovoAutoServer.Model
{
    public class ApiResponseModel
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public ApiResponseModel()
        {
        }
        public ApiResponseModel(JsonSerializerSettings jsonSerializerSettings)
        {
            _jsonSerializerSettings = jsonSerializerSettings;
        }
        public string ErrorMessage { get; set; }
        public bool IsSuccess { get; set; }
        public object Data { get; set; }

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this, _jsonSerializerSettings);
        }
    }
}
