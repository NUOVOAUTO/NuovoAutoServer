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
        public static void RegisterApiClient<T>(this IServiceCollection services, IAuthenticationProvider authenticationProvider) where T : class
        {
            var serviceProvider = services.BuildServiceProvider();
            using (var scope = serviceProvider.CreateScope())
            {
                services.AddSingleton(x => authenticationProvider);
                services.AddHttpClient<IApiClient<T>, ApiClient<T>>(x=> new ApiClient<T>(x, authenticationProvider))
                   .SetHandlerLifetime(TimeSpan.FromMinutes(5)).
                   AddRetryPolicy();
            }

        }
    }
}
