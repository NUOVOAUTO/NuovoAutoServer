using Azure;

using Microsoft.EntityFrameworkCore;

using NuovoAutoServer.Model;
using NuovoAutoServer.Repository.DBContext;
using NuovoAutoServer.Repository.Repository;
using NuovoAutoServer.Services.API_Provider;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace NuovoAutoServer.Services
{
    public class VehicleDetailsService
    {
        private readonly IGenericRepository<CosmosDBContext> _repo;
        private readonly IVehicleDetailsApiProvider _vehicleDetailsApiProvider;

        private bool IsExpired(DateTime dt)
        {
            return dt.AddSeconds(300) <= DateTime.Now;
        }

        public VehicleDetailsService(IGenericRepository<CosmosDBContext> repository, IVehicleDetailsApiProvider vehicleDetailsApiProvider)
        {
            _repo = repository;
            _vehicleDetailsApiProvider = vehicleDetailsApiProvider;
        }

        public async Task<VehicleDetails> GetByTagNumber2(string tagNumber, string state)
        {
            var query = _repo.Get<VehicleDetails>();
            query = query.Where(x => x.PartitionKey.StartsWith(tagNumber) && x.State == state);
            var res = await query.Select(x => x).FirstOrDefaultAsync();
            var isExpired = res != null ? !IsExpired(res.LastUpdatedDateTime) : true;
            if (isExpired || res == null)
            {
                var resultFromApi = await _vehicleDetailsApiProvider.GetByTagNumber(tagNumber, state);
                if (res != null)
                {
                    resultFromApi.Id = res.Id;

                    res = await _repo.UpdateAsync(resultFromApi);
                }
                else
                    res = await _repo.AddAsync(resultFromApi);
            }
            return res;
        }

        public async Task<VehicleDetails> GetByTagNumber(string tagNumber, string state)
        {
            // Query to get the vehicle details by tag number and state
            var vehicleDetails = await _repo.Get<VehicleDetails>()
                                            .Where(x => x.PartitionKey.StartsWith(tagNumber) && x.State == state)
                                            .FirstOrDefaultAsync();

            // Check if the vehicle details are expired
            bool isExpired = vehicleDetails != null && IsExpired(vehicleDetails.LastUpdatedDateTime);

            // If the vehicle details are expired or not found, get fresh details from API
            if (isExpired || vehicleDetails == null)
            {
                var freshDetails = await _vehicleDetailsApiProvider.GetByTagNumber(tagNumber, state);
              
                if (freshDetails == null)
                {
                    throw new Exception("Not able to fetch details");
                }

                var vinDetails = await _repo.Get<VehicleDetails>()
                                          .Where(x => x.PartitionKey.Contains(freshDetails.Vin) && x.Vin == freshDetails.Vin)
                                          .FirstOrDefaultAsync();

                if(vinDetails != null)
                {
                    await this._repo.RemoveAsync(vinDetails);
                }

                VehicleDetails? freshVinDetails = null;
                freshVinDetails = await _vehicleDetailsApiProvider.GetByVinNumber(freshDetails.Vin, freshDetails.LicenseNumber);
                freshVinDetails.LicenseNumber = freshDetails.LicenseNumber;
                freshVinDetails.State = freshDetails.State;
                vehicleDetails = await AddOrUpdateVehicleDetailsAsync(null, freshVinDetails);
                
                //if (vinDetails == null)
                //{
              
                //}
                //else
                //{
                //    vinDetails.LicenseNumber = freshDetails.LicenseNumber;
                //    vinDetails.State = freshDetails.State;

                //    await this._repo.RemoveAsync(vinDetails);

                //    vehicleDetails = await AddOrUpdateVehicleDetailsAsync(vinDetails, vinDetails);
                //}

                //vehicleDetails= await AddOrUpdateVehicleDetailsAsync(vinDetails, freshVinDetails);
                //freshDetails = await _vehicleDetailsApiProvider.GetByVinNumber(freshDetails.Vin);

                // If vehicle details already exist in the repository, update it
                //vehicleDetails = await AddOrUpdateVehicleDetailsAsync(vehicleDetails, freshDetails);
            }

            return vehicleDetails;
        }

        public async Task<VehicleDetails> GetByVinNumber(string vinNumber)
        {
            var vehicleDetails = await _repo.Get<VehicleDetails>()
                                            .Where(x => x.PartitionKey.Contains(vinNumber) && x.Vin == vinNumber)
                                            .FirstOrDefaultAsync();

            // Check if the vehicle details are expired
            bool isExpired = vehicleDetails != null && IsExpired(vehicleDetails.LastUpdatedDateTime);

            // If the vehicle details are expired or not found, get fresh details from API
            if (isExpired || vehicleDetails == null || vehicleDetails?.IsVinDetailsFetched == false)
            {
                var freshDetails = await _vehicleDetailsApiProvider.GetByVinNumber(vinNumber);

                if (freshDetails == null)
                {
                    throw new Exception("Not able to fetch details");
                }

                // If vehicle details already exist in the repository, update it
                vehicleDetails = await AddOrUpdateVehicleDetailsAsync(vehicleDetails, freshDetails);
            }

            return vehicleDetails;
        }

        private async Task<VehicleDetails?> AddOrUpdateVehicleDetailsAsync(VehicleDetails? vehicleDetails, VehicleDetails freshDetails)
        {
            if (vehicleDetails != null)
            {
                vehicleDetails.SetEntity(freshDetails);
                vehicleDetails = await _repo.UpdateAsync(vehicleDetails);
            }
            else // If vehicle details do not exist in the repository, add new details
            {
                freshDetails.Id = Guid.NewGuid();
                vehicleDetails = await _repo.AddAsync(freshDetails);
            }

            return vehicleDetails;
        }

        public async Task<VehicleDetails> UpsertVehicleDetails(VehicleDetails vehicleDetails)
        {
            return null;
        }
    }


}
