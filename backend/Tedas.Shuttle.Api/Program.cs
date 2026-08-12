using Serilog;
using Tedas.Shuttle.Api.Extensions;
using Tedas.Shuttle.Api.Middleware;
using Tedas.Shuttle.Application;
using Tedas.Shuttle.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    var paths = services.GetRequiredService<IApplicationDataPathProvider>();

    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(
            Path.Combine(paths.LogsDirectory, "tedas-shuttle-.log"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 14);
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiServices();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapHealthEndpoints();

app.MigrateDatabase();

app.Run();

public partial class Program;
