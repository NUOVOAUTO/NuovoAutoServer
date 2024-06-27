using Microsoft.ApplicationInsights;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using NuovoAutoServer.Api.Extensions;
using NuovoAutoServer.Shared;

using System.Text.Json;
using System.Text.Json.Serialization;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddSingleton<TelemetryClient>();

        var configuration = new ConfigurationBuilder()
     .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
   .AddEnvironmentVariables()
            .Build();

        var appSettings = configuration.GetSection("AppSettings");
        services.Configure<AppSettings>(appSettings);

        services.Configure<JsonSerializerOptions>(jsonSerializerOptions =>
             {
                 jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                 jsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                 jsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
                 // override the default value
                 jsonSerializerOptions.PropertyNameCaseInsensitive = false;
             });

        services.RegisterAll();

    })
    .Build();

host.Run();
