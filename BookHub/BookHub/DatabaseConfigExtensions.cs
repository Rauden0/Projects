using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Nest;

namespace BookHub;

public static class DatabaseConfigExtensions
{
    public static void AddConfiguredDatabase(this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment env)
    {
        services.AddDbContext<BookHubDbContext>(options =>
        {
            switch (env.IsDevelopment())
            {
                case true:
                    var dbPath = configuration["Database:Name"] ?? "BookHubDev.db";
                    options.UseSqlite($"Data Source={dbPath}").UseLazyLoadingProxies();
                    break;

                case false:
                    var dbSettings = configuration.GetSection("ConnectionStrings");
                    var connectionString = dbSettings.GetSection("BookHubConnection").Value;

                    options.UseSqlServer(connectionString).UseLazyLoadingProxies();
                    break;
            }
        });
    }

    public static void AddConfiguredElasticSearch(this IServiceCollection services,
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
    }
}