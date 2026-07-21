using VehicleSalesFIAP.Application.Abstractions.Clock;
using VehicleSalesFIAP.Application.Abstractions.Persistence;
using VehicleSalesFIAP.Application.Common.Exceptions;
using VehicleSalesFIAP.Domain.ValueObjects;

namespace VehicleSalesFIAP.Application.Vehicles.UpdateVehicle;

public sealed class UpdateVehicleUseCase(
    IVehicleRepository vehicleRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
{
    public async Task<VehicleResponse> HandleAsync(
        UpdateVehicleCommand command,
        CancellationToken cancellationToken = default)
    {
        var vehicle = await vehicleRepository.GetByIdAsync(command.Id, cancellationToken);
        if (vehicle is null)
        {
            throw new NotFoundException($"Vehicle '{command.Id}' was not found.");
        }

        vehicle.UpdateDetails(
            command.Brand,
            command.Model,
            command.Year,
            command.Color,
            Money.From(command.Price),
            dateTimeProvider.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return VehicleResponse.FromVehicle(vehicle);
    }
}
