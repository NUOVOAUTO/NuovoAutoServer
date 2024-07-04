using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NuovoAutoServer.Api.Extensions;
using NuovoAutoServer.Services;
using NuovoAutoServer.Shared;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit.DependencyInjection;

namespace NuovoAutoServer.Tests
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
  .AddEnvironmentVariables()
           .Build();
            services.AddSingleton<TelemetryClient>();
            services.AddLogging(builder =>
              {
                  builder.AddConsole(); // Add console logger
                  builder.AddDebug(); // Add debug logger
              });

            var appSettings = configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettings);
            services.RegisterAll();
        }
    }

    [Startup(typeof(Startup))]
    public class TestStartup
    {
        // This class doesn't need to contain any code
    }
}
