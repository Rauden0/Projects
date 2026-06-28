using BookHub.Mvc.Data;
using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Nest;

namespace BookHub.Mvc;

public static class DatabaseConfigExtensions
{
    public static void AddConfiguredDatabase(this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment env)
    {
        services.AddDbContext<BookHubDbContext>(options =>
        {
            var dbPath = configuration["Database:Name"] ?? "BookHubDev.db";
            options.UseSqlite($"Data Source={dbPath}").UseLazyLoadingProxies();
        });

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlite(configuration.GetConnectionString("IdentityDb"));
        });
    }
    public static IServiceCollection AddConfiguredElasticSearch(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IElasticClient>(sp =>
            {
                var config = configuration.GetSection("Elasticsearch");
                var url = config.GetValue<string>("Url");

                var settings = new ConnectionSettings(new Uri(url!));
                return new ElasticClient(settings);
            }
        );
        
        return services;
    }
}