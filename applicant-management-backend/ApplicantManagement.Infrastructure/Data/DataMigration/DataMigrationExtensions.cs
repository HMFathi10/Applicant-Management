using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ApplicantManagement.Infrastructure.Data.DataMigration
{
    public static class DataMigrationExtensions
    {
        public static async Task MigrateDataAsync(this IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            
            try
            {
                var dataMigration = services.GetRequiredService<ApplicantDataMigration>();
                await dataMigration.MigrateDataAsync();
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<IHost>>();
                logger.LogError(ex, "An error occurred during data migration");
            }
        }

        public static IServiceCollection AddDataMigration(this IServiceCollection services)
        {
            services.AddScoped<ApplicantDataMigration>();
            return services;
        }
    }
}