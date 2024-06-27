using NuovoAutoServer.Model;

namespace NuovoAutoServer.Services.API_Provider
{
    public interface IVehicleDetailsApiProvider
    {
        Task<VehicleDetails?> GetByTagNumber(string tagNumber, string state);
        Task<VehicleDetails> GetByVinNumber(string vinNumber, string tagNumber = "");
    }

}
