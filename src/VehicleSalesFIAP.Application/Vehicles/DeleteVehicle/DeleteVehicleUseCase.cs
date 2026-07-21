using VehicleSalesFIAP.Application.Abstractions.Persistence;
using VehicleSalesFIAP.Application.Common.Exceptions;

namespace VehicleSalesFIAP.Application.Vehicles.DeleteVehicle;

public sealed class DeleteVehicleUseCase(
    IVehicleRepository vehicleRepository,
    IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vehicle = await vehicleRepository.GetByIdAsync(id, cancellationToken);
        if (vehicle is null)
        {
            throw new NotFoundException($"Vehicle '{id}' was not found.");
        }

        vehicle.EnsureCanBeDeleted();
        vehicleRepository.Remove(vehicle);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
