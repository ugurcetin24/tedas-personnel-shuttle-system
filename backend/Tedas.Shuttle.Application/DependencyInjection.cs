using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Tedas.Shuttle.Application.Services;

namespace Tedas.Shuttle.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<IPersonnelService, PersonnelService>();
        services.AddScoped<IShuttleService, ShuttleService>();
        services.AddScoped<IShiftService, ShiftService>();
        services.AddScoped<IDriverService, DriverService>();
        services.AddScoped<IAssignmentService, AssignmentService>();
        services.AddScoped<IRoutePointService, RoutePointService>();
        services.AddScoped<IRouteCalculationService, RouteCalculationService>();
        services.AddScoped<IExcelImportPreviewService, ExcelImportPreviewService>();

        return services;
    }
}
