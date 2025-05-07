using VelvetPostBackEnd.Seeders;

namespace VelvetPostBackEnd.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static async Task SeedDatabaseAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
            await seeder.SeedAsync();
        }

        public static IServiceCollection AddDatabaseSeeder(this IServiceCollection services)
        {
            services.AddScoped<DatabaseSeeder>();
            return services;
        }
    }
}
