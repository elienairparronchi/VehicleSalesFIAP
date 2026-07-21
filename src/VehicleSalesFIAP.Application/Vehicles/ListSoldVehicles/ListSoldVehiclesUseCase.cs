using VehicleSalesFIAP.Application.Abstractions.Persistence;

namespace VehicleSalesFIAP.Application.Vehicles.ListSoldVehicles;

public sealed class ListSoldVehiclesUseCase(IVehicleRepository vehicleRepository)
{
    public async Task<IReadOnlyList<VehicleResponse>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var vehicles = await vehicleRepository.ListSoldOrderedByPriceAsync(cancellationToken);

        return vehicles
            .Select(VehicleResponse.FromVehicle)
            .ToList();
    }
}
