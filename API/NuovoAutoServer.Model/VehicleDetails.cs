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
            SetPartitionKey();
        }
        public Guid Id { get; set; }

        public string LicenseNumber { get; set; }
        public string Vin { get; set; }

        public string? State { get; set; }

        public JObject BasicDetails { get; set; }
        public bool? IsVinDetailsFetched { get; set; }

        public override void SetPartitionKey()
        {
            PartitionKey = $"{LicenseNumber}-{Vin}";
        }

        public VehicleDetails SetEntity(VehicleDetails source)
        {
            this.LicenseNumber = this.LicenseNumber ?? source.LicenseNumber;
            this.Vin = this.Vin ?? source.Vin;
            this.State = this.State ?? source.State;
            this.BasicDetails = source.BasicDetails;
            this.IsVinDetailsFetched = source.IsVinDetailsFetched;
            this.SetPartitionKey();

            this.OnChanged();

            return this;
        }
    }
}
