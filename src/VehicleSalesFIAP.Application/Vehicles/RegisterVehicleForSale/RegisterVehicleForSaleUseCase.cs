using VehicleSalesFIAP.Application.Abstractions.Clock;
using VehicleSalesFIAP.Application.Abstractions.Persistence;
using VehicleSalesFIAP.Domain.ValueObjects;
using VehicleSalesFIAP.Domain.Vehicles;

namespace VehicleSalesFIAP.Application.Vehicles.RegisterVehicleForSale;

public sealed class RegisterVehicleForSaleUseCase(
    IVehicleRepository vehicleRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
{
    public async Task<VehicleResponse> HandleAsync(
        RegisterVehicleForSaleCommand command,
        CancellationToken cancellationToken = default)
    {
        var vehicle = Vehicle.RegisterForSale(
            command.Brand,
            command.Model,
            command.Year,
            command.Color,
            Money.From(command.Price),
            dateTimeProvider.UtcNow);

        await vehicleRepository.AddAsync(vehicle, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return VehicleResponse.FromVehicle(vehicle);
    }
}
