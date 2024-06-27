using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NuovoAutoServer.Repository.DBContext;
using NuovoAutoServer.Shared;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NuovoAutoServer.Repository.Repository;

namespace NuovoAutoServer.Repository.Registrations
{
    public static class RepositoryRegistration
    {
        public static void RegisterRepositories(this IServiceCollection services)
        {
            services.AddDBContext();
            services.AddTransient<IGenericRepository<CosmosDBContext>,  CosmosDbEFRepository>();
        }

        private static void AddDBContext(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            using (var scope = serviceProvider.CreateScope())
            {
                var appSettings = scope.ServiceProvider.GetRequiredService<IOptions<AppSettings>>().Value;
                //services.AddDbContext<SqlDbContext>(options => options.UseSqlServer(appSettings.connectionStrings.SqlDbConnection, b => {
                //    b.MigrationsAssembly("MCAuditPro.Repositories");
                //    b.CommandTimeout(3);
                //}));

                services.AddDbContext<CosmosDBContext>();
            }

            services.CreateDatabases();
        }

        public static void CreateDatabases(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<CosmosDBContext>();
                Task.Run(async () => await dbContext.Database.EnsureCreatedAsync()).Wait();
            }
        }
    }
}
