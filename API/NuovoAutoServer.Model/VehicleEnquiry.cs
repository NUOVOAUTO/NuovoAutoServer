using System.ComponentModel.DataAnnotations;

namespace NuovoAutoServer.Model
{
    public class VehicleEnquiry : DomainModelBase
    {
        private string _vinNumber;

        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MinLength(1)]
        public string FullName { get; set; }

        [Required]
        [MinLength(1)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        [RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "Invalid Zip Code format.")]
        public string Zipcode { get; set; }

        [Required]
        [MinLength(1)]
        public string Make { get; set; }

        [Required]
        [MinLength(1)]
        public string Model { get; set; }

        [Required]
        [MinLength(1)]
        public string Year { get; set; }
        public string State { get; set; }
        public string LicenseNumber { get; set; }
        public DateTime SubmittedOn { get; set; }
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

        public IEnumerable<ValidationResult> Validate()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                yield return new ValidationResult("Email cannot be empty or whitespace.", new[] { nameof(Email) });
            }

            if (string.IsNullOrWhiteSpace(FullName))
            {
                yield return new ValidationResult("FullName cannot be empty or whitespace.", new[] { nameof(FullName) });
            }

            if (string.IsNullOrWhiteSpace(Zipcode))
            {
                yield return new ValidationResult("ZipCode cannot be empty or whitespace.", new[] { nameof(Zipcode) });
            }

            if (string.IsNullOrWhiteSpace(PhoneNumber))
            {
                yield return new ValidationResult("PhoneNumber cannot be empty or whitespace.", new[] { nameof(PhoneNumber) });
            }

            if (string.IsNullOrWhiteSpace(VinNumber))
            {
                yield return new ValidationResult("VinNumber cannot be empty or whitespace.", new[] { nameof(VinNumber) });
            }

            if (string.IsNullOrWhiteSpace(Make))
            {
                yield return new ValidationResult("Make cannot be empty or whitespace.", new[] { nameof(Make) });
            }

            if (string.IsNullOrWhiteSpace(Model))
            {
                yield return new ValidationResult("Model cannot be empty or whitespace.", new[] { nameof(Model) });
            }

            if (string.IsNullOrWhiteSpace(Year))
            {
                yield return new ValidationResult("Year cannot be empty or whitespace.", new[] { nameof(Year) });
            }

            if (string.IsNullOrWhiteSpace(VehicleEnquiryDetails?.CurrentMileage))
            {
                yield return new ValidationResult("CurrentMileage cannot be empty or whitespace.", new[] { nameof(VehicleEnquiryDetails.CurrentMileage) });
            }
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
