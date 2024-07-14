using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

using NuovoAutoServer.Model;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuovoAutoServer.Repository.DBContext
{
    public class SqlDbContext : DbContext
    {
        public DbSet<VehicleEnquiry> VehicleEnquiry { get; set; }
        public DbSet<VehicleEnquiryDetails> VehicleEnquiryDetails { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var cs = Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING");
            optionsBuilder.UseSqlServer(cs, sqlServerOptionsAction: sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3, // Maximum number of retries
                    maxRetryDelay: TimeSpan.FromSeconds(30), // Maximum delay between retries
                    errorNumbersToAdd: null); // SQL error numbers to consider as transient
            });
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VehicleEnquiry>().ToTable(nameof(VehicleEnquiry));
            modelBuilder.Entity<VehicleEnquiryDetails>().ToTable(nameof(VehicleEnquiryDetails));

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                DbContextHelper.ConfigureIdProperty(modelBuilder, entityType);
            }

            modelBuilder.Entity<VehicleEnquiry>().HasKey(x => x.Id);
            modelBuilder.Entity<VehicleEnquiryDetails>().HasKey(x => x.Id);

            modelBuilder.Entity<VehicleEnquiry>()
                              .Property(x => x.EnquiryStatus)
                              .HasDefaultValue("Pending");

            modelBuilder.Entity<VehicleEnquiryDetails>()
                .HasOne(cga => cga.VehicleEnquiry)
                .WithOne(cg => cg.VehicleEnquiryDetails)
                .HasForeignKey<VehicleEnquiryDetails>(cga => cga.VehicleEnquiryId); // Ensure foreign key is set correctly
        }
    }
}
