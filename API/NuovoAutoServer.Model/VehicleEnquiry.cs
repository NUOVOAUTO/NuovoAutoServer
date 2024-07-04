namespace NuovoAutoServer.Model
{
    public class VehicleEnquiry : DomainModelBase
    {
        private string _vinNumber;

        public string Id
        {
            get => _vinNumber;
            set => _vinNumber = value;
        }

        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Year { get; set; }
        public string State { get; set; }
        public string LicenseNumber { get; set; }
        public string VinNumber
        {
            get => _vinNumber;
            set => _vinNumber = value;
        }
        public VehicleEnquiryDetails VehicleEnquiryDetails { get; set; }

        public VehicleEnquiry()
        {
            SetPartitionKey();
        }

        public override string SetPartitionKey()
        {
            return PartitionKey = $"{State}";
        }
    }
    public class VehicleEnquiryDetails
    {
        public string Style { get; set; }
        public string Drivetrain { get; set; }
        public string Transmission { get; set; }
        public List<string> Features { get; set; }
        public string CurrentMileage { get; set; }
        public string AddtionalInfo { get; set; }
        public string HasBeenInAccident { get; set; }
        public string HasFrameDamage { get; set; }
        public string HasFloodDamage { get; set; }
        public string HasBeenSmokedIn { get; set; }
        public string HasMechanicalIssues { get; set; }
        public string HasBrokenOdometer { get; set; }
        public string HasPanelsInNeedOfRepair { get; set; }
        public string HasRustOrHailDamage { get; set; }
        public string HasBrokenInteriorParts { get; set; }
        public string HasMinorInteriorIssues { get; set; }
        public string NeedTireReplacement { get; set; }
        public string NumberOfKeys { get; set; }
        public string HasAftermarketModifications { get; set; }
        public string HasOtherIssues { get; set; }
        public string OtherIssuesDescription { get; set; }
        public string ConditionDescription { get; set; }
    }
}
