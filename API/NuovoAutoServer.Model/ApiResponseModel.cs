using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuovoAutoServer.Model
{
    public class ApiResponseModel
    {
        public string ErrorMessage { get; set; }
        public bool IsSuccess { get; set; }
        public object Data { get; set; }
    }
}
