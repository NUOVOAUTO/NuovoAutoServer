using Microsoft.ApplicationInsights;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Newtonsoft.Json;

using NuovoAutoServer.Api.Extensions;
using NuovoAutoServer.Services.EmailNotification;
using NuovoAutoServer.Shared;

using System.Text.Json;
using System.Text.Json.Serialization;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddSingleton<TelemetryClient>();

        var env = Environment.GetEnvironmentVariable("ENVIRONMENT");
        var emailTemplateConfig = "EmailNotification/emailTemplates.json";
        if(env == "PROD")
        {
            emailTemplateConfig = "EmailNotification/emailTemplates.prod.json";
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) // Set the base path
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddJsonFile(emailTemplateConfig, optional: false, reloadOnChange: true) // Add the correct path to emailTemplates.json
            .AddEnvironmentVariables()
            .Build();

        var appSettings = configuration.GetSection("AppSettings");
        services.Configure<AppSettings>(appSettings);

        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings")); // Configure EmailSettings

        services.Configure<JsonSerializerOptions>(jsonSerializerOptions =>
        {
            jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            jsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            jsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            // override the default value
            jsonSerializerOptions.PropertyNameCaseInsensitive = false;
        });

        services.Configure<JsonSerializerSettings>(jsonSerializerSettings =>
        {
            jsonSerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
            jsonSerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            jsonSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            // override the default value
            jsonSerializerSettings.Formatting = Formatting.None;
        });

        services.AddAzureClients(builder =>
        {
            var sbcs = configuration.GetValue<string>("ServiceBusConnection");
            builder.AddServiceBusAdministrationClient(sbcs);
            builder.AddServiceBusClient(sbcs);

            var cs = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            builder.AddBlobServiceClient(cs);
        });

        services.RegisterAll();
    })
    .Build();

host.Run();
