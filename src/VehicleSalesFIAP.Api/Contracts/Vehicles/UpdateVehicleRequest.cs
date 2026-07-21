namespace VehicleSalesFIAP.Api.Contracts.Vehicles;

public sealed record UpdateVehicleRequest(
    string Brand,
    string Model,
    int Year,
    string Color,
    decimal Price);
