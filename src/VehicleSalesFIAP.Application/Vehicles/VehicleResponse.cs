using VehicleSalesFIAP.Domain.Vehicles;

namespace VehicleSalesFIAP.Application.Vehicles;

public sealed record VehicleResponse(
    Guid Id,
    string Brand,
    string Model,
    int Year,
    string Color,
    decimal Price,
    string Currency,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? SoldAt)
{
    public static VehicleResponse FromVehicle(Vehicle vehicle)
    {
        return new VehicleResponse(
            vehicle.Id,
            vehicle.Brand,
            vehicle.Model,
            vehicle.Year,
            vehicle.Color,
            vehicle.Price.Amount,
            vehicle.Price.Currency,
            vehicle.Status.ToString(),
            vehicle.CreatedAt,
            vehicle.UpdatedAt,
            vehicle.SoldAt);
    }
}
