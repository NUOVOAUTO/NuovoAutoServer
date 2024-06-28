using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NuovoAutoServer.Repository.Registrations;
using NuovoAutoServer.Services.API_Provider;
using NuovoAutoServer.Services;
using Rest.ApiClient.Extensions.Registrations;
using Rest.ApiClient.Auth;
using Microsoft.Extensions.Options;
using NuovoAutoServer.Shared;


namespace NuovoAutoServer.Api.Extensions
{
    public static class RegisterServices
    {
        public static void RegisterAll(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();

            services.RegisterRepositories();

            services.AddTransient<VehicleDetailsService>();
            services.AddSingleton<SecurityService>();
            services.AddTransient<IVehicleDetailsApiProvider, VehicleDatabaseApiProvider>();

            using (var scope = serviceProvider.CreateScope())
            {
                var appSettings = scope.ServiceProvider.GetRequiredService<IOptions<AppSettings>>().Value;
                var authKey = appSettings.VehicleDatabasesApiProvider.AuthKey;
                services.AddSingleton(x => new CustomAuthenticationHeaderProvider("x-AuthKey", authKey));
            }
            services.RegisterApiClient();
        }
    }
}
