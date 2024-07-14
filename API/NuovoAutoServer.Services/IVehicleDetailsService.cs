using NuovoAutoServer.Model;

namespace NuovoAutoServer.Services
{
    public interface IVehicleDetailsService
    {
        Task<VehicleDetails> GetByTagNumber(string tagNumber, string state);
        Task<VehicleDetails> GetByVinNumber(string vinNumber);
    }
}