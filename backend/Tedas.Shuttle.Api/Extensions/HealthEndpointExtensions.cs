using Microsoft.EntityFrameworkCore;
using Tedas.Shuttle.Infrastructure;
using Tedas.Shuttle.Infrastructure.Persistence;

namespace Tedas.Shuttle.Api.Extensions;

public static class HealthEndpointExtensions
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/health", async (
            AppDbContext dbContext,
            IApplicationDataPathProvider paths,
            CancellationToken cancellationToken) =>
        {
            var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

            return Results.Ok(new
            {
                application = "TEDAŞ Personel Servisi Atama Sistemi",
                status = "Healthy",
                database = new
                {
                    provider = dbContext.Database.ProviderName,
                    canConnect,
                    path = paths.DatabasePath
                }
            });
        })
        .WithName("Health")
        .WithTags("System");

        return endpoints;
    }
}
