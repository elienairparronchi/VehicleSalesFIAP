using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using VehicleSalesFIAP.Application.Abstractions.Persistence;
using VehicleSalesFIAP.Application.Common.Exceptions;
using VehicleSalesFIAP.Domain.Sales;
using VehicleSalesFIAP.Domain.Vehicles;

namespace VehicleSalesFIAP.Infrastructure.Persistence;

public sealed class VehicleSalesDbContext(DbContextOptions<VehicleSalesDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    public DbSet<Sale> Sales => Set<Sale>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new ConflictException(
                "The resource was changed by another request. Reload it and try again.",
                exception);
        }
        catch (DbUpdateException exception)
            when (exception.GetBaseException() is SqlException { Number: 2601 or 2627 })
        {
            throw new ConflictException("The vehicle has already been sold.", exception);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(VehicleSalesDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
