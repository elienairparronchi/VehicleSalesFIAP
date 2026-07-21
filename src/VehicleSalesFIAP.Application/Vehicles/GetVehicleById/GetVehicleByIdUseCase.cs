using VehicleSalesFIAP.Application.Abstractions.Persistence;
using VehicleSalesFIAP.Application.Common.Exceptions;

namespace VehicleSalesFIAP.Application.Vehicles.GetVehicleById;

public sealed class GetVehicleByIdUseCase(IVehicleRepository vehicleRepository)
{
    public async Task<VehicleResponse> HandleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vehicle = await vehicleRepository.GetByIdAsync(id, cancellationToken);
        if (vehicle is null)
        {
            throw new NotFoundException($"Vehicle '{id}' was not found.");
        }

        return VehicleResponse.FromVehicle(vehicle);
    }
}
