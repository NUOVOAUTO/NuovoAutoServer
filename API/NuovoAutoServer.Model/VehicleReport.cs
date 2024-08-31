using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuovoAutoServer.Model
{
    public class VehicleReport
    {
        public VehicleReport(string vin, JObject? data)
        {
            Vin = vin;
            Html = data?["html"]?.ToString();
        }

        public VehicleReport() { }

        public string Vin { get; set; }

        public string Html { get; set; }
        public string BlobPath { get; set; }

        //public byte[] ByteContent { get; set; }

    }
}
