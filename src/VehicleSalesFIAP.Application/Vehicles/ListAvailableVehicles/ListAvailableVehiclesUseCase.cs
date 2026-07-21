using VehicleSalesFIAP.Application.Abstractions.Persistence;

namespace VehicleSalesFIAP.Application.Vehicles.ListAvailableVehicles;

public sealed class ListAvailableVehiclesUseCase(IVehicleRepository vehicleRepository)
{
    public async Task<IReadOnlyList<VehicleResponse>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var vehicles = await vehicleRepository.ListAvailableOrderedByPriceAsync(cancellationToken);

        return vehicles
            .Select(VehicleResponse.FromVehicle)
            .ToList();
    }
}
