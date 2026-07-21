using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VehicleSalesFIAP.Application.Abstractions.Clock;
using VehicleSalesFIAP.Application.Abstractions.Persistence;
using VehicleSalesFIAP.Infrastructure.Clock;
using VehicleSalesFIAP.Infrastructure.Persistence;
using VehicleSalesFIAP.Infrastructure.Persistence.Repositories;

namespace VehicleSalesFIAP.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("VehicleSales");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'VehicleSales' was not configured.");
        }

        services.AddDbContext<VehicleSalesDbContext>(options =>
        {
            options.UseSqlServer(
                connectionString,
                sqlOptions => sqlOptions.MigrationsAssembly(typeof(VehicleSalesDbContext).Assembly.FullName));
        });

        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<VehicleSalesDbContext>());
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        return services;
    }
}
