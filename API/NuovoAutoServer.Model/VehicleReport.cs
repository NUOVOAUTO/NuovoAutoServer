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
        public VehicleReport(string vin, byte[] content)
        {
            Vin = vin;
            Content = content;
        }

        public VehicleReport() { }

        public string Vin { get; set; }

        public byte[] Content { get; set; }

        //public byte[] ByteContent { get; set; }

    }
}
