using VehicleSalesFIAP.Domain.Vehicles;

namespace VehicleSalesFIAP.Application.Abstractions.Persistence;

public interface IVehicleRepository
{
    Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken = default);

    void Remove(Vehicle vehicle);

    Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Vehicle>> ListAvailableOrderedByPriceAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Vehicle>> ListSoldOrderedByPriceAsync(CancellationToken cancellationToken = default);
}
