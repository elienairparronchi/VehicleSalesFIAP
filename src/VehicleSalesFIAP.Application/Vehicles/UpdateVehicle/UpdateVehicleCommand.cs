namespace VehicleSalesFIAP.Application.Vehicles.UpdateVehicle;

public sealed record UpdateVehicleCommand(
    Guid Id,
    string Brand,
    string Model,
    int Year,
    string Color,
    decimal Price);
