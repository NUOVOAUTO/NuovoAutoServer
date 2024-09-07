using NuovoAutoServer.Model;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuovoAutoServer.Services.API_Provider
{
    public interface IVehicleReportApiProvider
    {
        Task<VehicleReport> DownloadVinReport(string vin);
    }
}
