using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using NuovoAutoServer.Model;
using NuovoAutoServer.Shared;

namespace NuovoAutoServer.Repository.DBContext
{
    public class CosmosDBContext : DbContext
    {
        public DbSet<VehicleDetails> VehicleDetails { get; set; }

        private readonly AppSettings _appSettingsOptions;
        private readonly ILoggerFactory _logger;

        public CosmosDBContext(IOptions<AppSettings> appSettingsOptions, ILoggerFactory logger)
        {
            _appSettingsOptions = appSettingsOptions.Value;
            _logger = logger;
        }
        #region Configuration
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseCosmos(
                _appSettingsOptions.ConnectionStrings.CosmosDbContextConnection.Name,
                 _appSettingsOptions.ConnectionStrings.CosmosDbContextConnection.DbName

            );
            if (_appSettingsOptions.LogDbDiagnosticts)
            {
                //optionsBuilder.EnableSensitiveDataLogging().UseLoggerFactory(_logger);
                optionsBuilder.EnableSensitiveDataLogging().LogTo(message =>
                {
                    if (message.Contains("CosmosEventId"))
                    {
                        var properties = ExtractDetails(message);
                        if (properties.Count > 0)
                        {
                            // _telemetryClient.TrackEvent("EFCore Cosmos Operation", properties: properties);
                        }
                    }
                }, LogLevel.Debug).UseLoggerFactory(_logger);
            }
        }

        #endregion


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region DefaultContainer
            modelBuilder.HasDefaultContainer("default");
            #endregion

            #region NoDiscriminator
            modelBuilder.Entity<VehicleDetails>()
                .HasDiscriminator();

            modelBuilder.Entity<VehicleEnquiry>()
               .HasDiscriminator();
            #endregion

            #region PartitionKey
            modelBuilder.Entity<VehicleDetails>()
                .HasPartitionKey(o => o.PartitionKey);

            modelBuilder.Entity<VehicleEnquiry>()
                .HasPartitionKey(o => o.PartitionKey);
            #endregion

            #region ETag
            modelBuilder.Entity<VehicleDetails>()
                .UseETagConcurrency();
            modelBuilder.Entity<VehicleEnquiry>()
                .UseETagConcurrency();
            #endregion

            #region Indexing

            modelBuilder.Entity<VehicleEnquiry>().HasIndex(o => o.VinNumber);
            modelBuilder.Entity<VehicleEnquiry>().HasAlternateKey(o => o.Email);
            modelBuilder.Entity<VehicleEnquiry>().HasIndex(o => o.Email).IsUnique();

            #endregion

            #region RequiredProperties

            modelBuilder.Entity<VehicleDetails>()
                .Property(o => o.Vin)
                .IsRequired();

            modelBuilder.Entity<VehicleEnquiry>()
                .Property(o => o.FullName)
                .IsRequired();

            modelBuilder.Entity<VehicleEnquiry>()
                .Property(o => o.Email)
                .IsRequired();

            modelBuilder.Entity<VehicleEnquiry>()
                .Property(o => o.PhoneNumber)
                .IsRequired().HasMaxLength(15);

            modelBuilder.Entity<VehicleEnquiry>()
                .Property(o => o.Zipcode)
                .IsRequired();

            #endregion

            #region PropertyNames
            modelBuilder.Entity<VehicleDetails>()
             .Property(o => o.BasicDetails)
             .HasConversion(
            v => JsonConvert.SerializeObject(v),
            v => JObject.Parse(v));


            modelBuilder.Entity<VehicleEnquiry>().OwnsOne(
                o => o.VehicleEnquiryDetails,
                sa =>
                {
                    sa.ToJsonProperty("Details");
                }
                );

            //modelBuilder.Entity<Order>().OwnsOne(
            //    o => o.ShippingAddress,
            //    sa =>
            //    {
            //        sa.ToJsonProperty("Address");
            //        sa.Property(p => p.Street).ToJsonProperty("ShipsToStreet");
            //        sa.Property(p => p.City).ToJsonProperty("ShipsToCity");
            //    });
            #endregion

            #region OwnsMany
            //modelBuilder.Entity<Distributor>().OwnsMany(p => p.ShippingCenters);
            #endregion

            #region ETagProperty
            //modelBuilder.Entity<Distributor>()
            //    .Property(d => d.ETag)
            //    .IsETagConcurrency();
            #endregion
        }

        private Dictionary<string, string> ExtractDetails(string message)
        {
            var properties = new Dictionary<string, string>();

            // Extract CosmosEventId
            var cosmosEventPattern = @"CosmosEventId\.(?<eventId>\w+)";
            var cosmosEventMatch = TryMatch(message, cosmosEventPattern, "eventId");

            if (cosmosEventMatch != null && CosmosEventId.ExecutingSqlQuery.ToString().Contains(cosmosEventMatch))
            {
                return properties;
            }

            // Patterns and their corresponding property keys
            var patterns = new Dictionary<string, string>
    {
        { @"(?<time>[\d.]+) ms,", "TimeTaken" },
        { @"(?<requestUnits>[\d.]+) RU\)", "RequestUnits" },
        { @"Container='(?<container>[^']+)'", "Container" },
        { @"Partition='(?<partition>[^']+)'", "Partition" },
        { @" Id='(?<itemId>[^']+)'", "ItemId" }
    };

            properties["CosmosEventId"] = cosmosEventMatch;

            // Match patterns and populate properties
            foreach (var pattern in patterns)
            {
                var matchValue = TryMatch(message, pattern.Key, pattern.Value);
                properties[pattern.Value] = matchValue;
            }

            return properties;
        }

        private string TryMatch(string message, string pattern, string groupName)
        {
            var match = Regex.Match(message, pattern);
            return match.Success ? match.Groups[groupName].Value : String.Empty;
        }

    }
}
