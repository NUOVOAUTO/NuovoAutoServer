using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuovoAutoServer.Model
{
    public class VehicleDetails : DomainModelBase
    {
        public VehicleDetails()
        {
            SetPartitionKey();
        }
        public VehicleDetails(string licenseNumber, string vin, JObject? data)
        {
            LicenseNumber = licenseNumber;
            Vin = vin;
            BasicDetails = data ?? new JObject();
            if (data != null)
            {
                Make = data?["basic"]?["make"]?.ToString();
                Model = data?["basic"]?["model"]?.ToString();
                Year = data?["basic"]?["year"]?.ToString();
                Transmission = data?["transmission"]?["transmission_style"]?.ToString();
                Drivetrain = data?["drivetrain"]?["drive_type"]?.ToString();
            }
            SetPartitionKey();
        }
        public string Id { get; set; }

        public string LicenseNumber { get; set; }
        public string Vin { get; set; }

        public string? StateCode { get; set; }
        public string? Make { get; set; }
        public string? Model { get; set; }
        public string? Year { get; set; }
        public string? Transmission { get; set; }
        public string? Drivetrain { get; set; }

        public JObject BasicDetails { get; set; }
        public bool? IsVinDetailsFetched { get; set; }

        public override string SetPartitionKey()
        {
            return PartitionKey = $"{LicenseNumber}-{Vin}";
        }

        public VehicleDetails SetEntity(VehicleDetails source)
        {
            this.LicenseNumber = this.LicenseNumber ?? source.LicenseNumber;
            this.Vin = this.Vin ?? source.Vin;
            this.StateCode = this.StateCode ?? source.StateCode;
            this.BasicDetails = source.BasicDetails;
            this.IsVinDetailsFetched = source.IsVinDetailsFetched;
            this.SetPartitionKey();

            this.OnChanged();

            return this;
        }
    }


}
