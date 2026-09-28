using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RubQue.Application.Auth.Interfaces;
using RubQue.Application.Common.Interfaces;
using RubQue.Infrastructure.Persistence;
using RubQue.Infrastructure.Security;

namespace RubQue.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var useInMemory = !bool.TryParse(configuration["UseInMemoryDatabase"], out var isInMemory) || isInMemory;

        if (useInMemory)
        {
            services.AddDbContext<RubQueDbContext>(options =>
                options.UseInMemoryDatabase("RubQueDb"));
        }
        else
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Host=localhost;Port=5432;Database=rubque_db;Username=postgres;Password=postgrespassword";

            services.AddDbContext<RubQueDbContext>(options =>
                options.UseNpgsql(connectionString, b =>
                    b.MigrationsAssembly(typeof(RubQueDbContext).Assembly.FullName)));
        }

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<RubQueDbContext>());

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}
