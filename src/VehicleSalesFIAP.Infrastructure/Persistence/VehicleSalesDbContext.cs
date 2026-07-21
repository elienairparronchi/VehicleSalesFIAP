using Microsoft.EntityFrameworkCore;
using VehicleSalesFIAP.Application.Abstractions.Persistence;
using VehicleSalesFIAP.Domain.Sales;
using VehicleSalesFIAP.Domain.Vehicles;

namespace VehicleSalesFIAP.Infrastructure.Persistence;

public sealed class VehicleSalesDbContext(DbContextOptions<VehicleSalesDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    public DbSet<Sale> Sales => Set<Sale>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(VehicleSalesDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
