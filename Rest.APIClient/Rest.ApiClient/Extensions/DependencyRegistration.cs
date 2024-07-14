using Rest.ApiClient.Auth;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rest.ApiClient.Extensions;

namespace Rest.ApiClient.Extensions.Registrations
{
    public static class DependencyRegistration
    {
        public static void RegisterApiClient(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            using (var scope = serviceProvider.CreateScope())
            {
                var azureAdAuthenticationProvider = scope.ServiceProvider.GetService<AzureAdAuthenticationProvider>();
                var customAuthenticationHeaderProvider = scope.ServiceProvider.GetService<CustomAuthenticationHeaderProvider>();

                services.AddHttpClient<IApiClient, ApiClient>(x=> new ApiClient(x, azureAdAuthenticationProvider, customAuthenticationHeaderProvider))
                   .SetHandlerLifetime(TimeSpan.FromMinutes(5)).
                   AddRetryPolicy();
            }

        }
    }
}
