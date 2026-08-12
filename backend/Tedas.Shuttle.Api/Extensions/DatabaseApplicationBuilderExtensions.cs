using Microsoft.EntityFrameworkCore;
using Tedas.Shuttle.Infrastructure.Persistence;

namespace Tedas.Shuttle.Api.Extensions;

public static class DatabaseApplicationBuilderExtensions
{
    public static IApplicationBuilder MigrateDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        logger.LogInformation("Starting TEDAŞ Shuttle database migration.");
        dbContext.Database.Migrate();
        logger.LogInformation("TEDAŞ Shuttle database migration completed.");

        return app;
    }
}
