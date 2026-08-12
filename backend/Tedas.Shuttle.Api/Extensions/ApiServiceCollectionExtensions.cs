namespace Tedas.Shuttle.Api.Extensions;

public static class ApiServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new()
            {
                Title = "TEDAŞ Personel Servisi Atama Sistemi API",
                Version = "v1",
                Description = "TEDAŞ Personel Servisi Atama Sistemi için ASP.NET Core Web API."
            });
        });

        services.AddProblemDetails();

        return services;
    }
}
