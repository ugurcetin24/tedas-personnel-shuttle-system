using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tedas.Shuttle.Infrastructure.Geocoding;
using Tedas.Shuttle.Application.Interfaces;
using Tedas.Shuttle.Infrastructure.Persistence;
using Tedas.Shuttle.Infrastructure.Repositories;

namespace Tedas.Shuttle.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IApplicationDataPathProvider, ApplicationDataPathProvider>();

        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            var paths = serviceProvider.GetRequiredService<IApplicationDataPathProvider>();
            paths.EnsureDirectoriesExist();

            var configuredConnectionString = configuration.GetConnectionString("Default");
            var connectionString = string.IsNullOrWhiteSpace(configuredConnectionString)
                ? paths.DatabaseConnectionString
                : configuredConnectionString;

            options.UseSqlite(connectionString);
        });

        services.AddScoped<IPersonnelRepository, PersonnelRepository>();
        services.AddScoped<IShuttleRepository, ShuttleRepository>();
        services.AddScoped<IShiftRepository, ShiftRepository>();
        services.AddScoped<IDriverRepository, DriverRepository>();
        services.AddScoped<IAssignmentRepository, AssignmentRepository>();
        services.AddScoped<IRoutePointRepository, RoutePointRepository>();

        services.AddHttpClient<IGeocodingService, NominatimGeocodingService>(client =>
        {
            var baseUrl = configuration["ExternalServices:Nominatim:BaseUrl"]
                ?? "https://nominatim.openstreetmap.org";

            client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("TedasPersonnelShuttleSystem/1.0");
        });

        return services;
    }
}
