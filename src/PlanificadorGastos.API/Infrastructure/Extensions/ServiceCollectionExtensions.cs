using PlanificadorGastos.API.Infrastructure.Authentication;
using PlanificadorGastos.API.Services.Implementations;
using PlanificadorGastos.API.Services.Interfaces;

namespace PlanificadorGastos.API.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Authentication
        services.AddScoped<IJwtService, JwtService>();

        // Application Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICategoriasService, CategoriasService>();
        services.AddScoped<IGastosService, GastosService>();
        services.AddScoped<IPresupuestosService, PresupuestosService>();
        services.AddScoped<IReportesService, ReportesService>();
        services.AddScoped<IIngresosService, IngresosService>();
        services.AddScoped<ILimitesCategoriasService, LimitesCategoriasService>();

        // HttpContext accessor para obtener el usuario actual
        services.AddHttpContextAccessor();

        // Current User Service
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}
