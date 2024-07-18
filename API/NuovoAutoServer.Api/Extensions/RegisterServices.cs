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
using NuovoAutoServer.Services.EmailNotification;
using Azure.Messaging.ServiceBus.Administration;


namespace NuovoAutoServer.Api.Extensions
{
    public static class RegisterServices
    {
        public static void RegisterAll(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();

            services.RegisterRepositories();

            //services.AddTransient<VehicleDetailsService>();
            //services.AddTransient<VehicleEnquiryService>();
            services.AddTransient<VehicleDetailsServiceSQL>();
            services.AddTransient<VehicleEnquiryServiceSQL>();
            services.AddTransient<EmailNotificationService>();
            services.AddSingleton<SecurityService>();
            services.AddSingleton<RetryHandler>();
            services.AddSingleton<AzureServiceBusClient>();
            services.AddTransient<IVehicleDetailsApiProvider, VehicleDatabaseApiProvider>();

            using (var scope = serviceProvider.CreateScope())
            {
                var appSettings = scope.ServiceProvider.GetRequiredService<IOptions<AppSettings>>().Value;
                var authKey = appSettings.VehicleDatabasesApiProvider.AuthKey;
                services.AddSingleton(x => new CustomAuthenticationHeaderProvider("x-AuthKey", authKey));
            }
            services.RegisterApiClient();

            using (var scope = serviceProvider.CreateScope())
            {
                var serviceBusAdminClient = scope.ServiceProvider.GetRequiredService<ServiceBusAdministrationClient>();
                var azureServiceBusClient = new AzureServiceBusClient(null, serviceBusAdminClient);
                Task.Run( async () => await azureServiceBusClient.InitializeQueueAsync()).Wait();
                Console.WriteLine("Service Bus Queue Initialized");
            }
        }
    }
}
