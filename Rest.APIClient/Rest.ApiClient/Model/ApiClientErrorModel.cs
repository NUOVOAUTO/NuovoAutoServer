using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Rest.ApiClient.Model
{
    public class ApiClientErrorModel
    {
        public string? ReasonPhrase { get; set; }
        public object Message { get; set; }
        public HttpStatusCode StatusCode { get; set; }

        public string Description { get; set; }
        public Exception Exception { get; set; }
    }
}
