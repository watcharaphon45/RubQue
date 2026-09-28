using Microsoft.Extensions.DependencyInjection;
using RubQue.Application.Auth.Interfaces;
using RubQue.Application.Auth.Services;

namespace RubQue.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}
