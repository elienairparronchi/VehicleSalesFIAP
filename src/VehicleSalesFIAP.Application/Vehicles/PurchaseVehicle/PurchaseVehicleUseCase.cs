using VehicleSalesFIAP.Application.Abstractions.Clock;
using VehicleSalesFIAP.Application.Abstractions.Persistence;
using VehicleSalesFIAP.Application.Common.Exceptions;

namespace VehicleSalesFIAP.Application.Vehicles.PurchaseVehicle;

public sealed class PurchaseVehicleUseCase(
    IVehicleRepository vehicleRepository,
    ISaleRepository saleRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
{
    public async Task<PurchaseVehicleResponse> HandleAsync(
        PurchaseVehicleCommand command,
        CancellationToken cancellationToken = default)
    {
        var vehicle = await vehicleRepository.GetByIdAsync(command.VehicleId, cancellationToken);
        if (vehicle is null)
        {
            throw new NotFoundException($"Vehicle '{command.VehicleId}' was not found.");
        }

        var sale = vehicle.SellTo(command.BuyerId, dateTimeProvider.UtcNow);

        await saleRepository.AddAsync(sale, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return PurchaseVehicleResponse.FromSale(sale);
    }
}
