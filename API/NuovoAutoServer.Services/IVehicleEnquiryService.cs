using NuovoAutoServer.Model;

namespace NuovoAutoServer.Services
{
    public interface IVehicleEnquiryService
    {
        Task<(IEnumerable<VehicleEnquiry> Items, int TotalCount)> GetPaginatedAsync(int start, int pageSize);

        Task<VehicleEnquiry> GetVehicleEnquiry(Guid id);
        Task SaveVehicleEnquiry(VehicleEnquiry vehicleEnquiry);
        Task SaveVehicleEnquiryBulk(List<VehicleEnquiry> vehicleEnquiries);
    }
}