using Microsoft.EntityFrameworkCore;
using VehicleSalesFIAP.Application.Abstractions.Persistence;
using VehicleSalesFIAP.Domain.Vehicles;

namespace VehicleSalesFIAP.Infrastructure.Persistence.Repositories;

internal sealed class VehicleRepository(VehicleSalesDbContext dbContext) : IVehicleRepository
{
    public async Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
    {
        await dbContext.Vehicles.AddAsync(vehicle, cancellationToken);
    }

    public void Remove(Vehicle vehicle)
    {
        dbContext.Vehicles.Remove(vehicle);
    }

    public async Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Vehicles
            .FirstOrDefaultAsync(vehicle => vehicle.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Vehicle>> ListAvailableOrderedByPriceAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Vehicles
            .AsNoTracking()
            .Where(vehicle => vehicle.Status == VehicleStatus.Available)
            .OrderBy(vehicle => vehicle.Price.Amount)
            .ThenBy(vehicle => vehicle.Brand)
            .ThenBy(vehicle => vehicle.Model)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Vehicle>> ListSoldOrderedByPriceAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Vehicles
            .AsNoTracking()
            .Where(vehicle => vehicle.Status == VehicleStatus.Sold)
            .OrderBy(vehicle => vehicle.Price.Amount)
            .ThenBy(vehicle => vehicle.Brand)
            .ThenBy(vehicle => vehicle.Model)
            .ToListAsync(cancellationToken);
    }
}
