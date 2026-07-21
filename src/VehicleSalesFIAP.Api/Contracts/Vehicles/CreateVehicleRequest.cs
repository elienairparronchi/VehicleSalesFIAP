namespace VehicleSalesFIAP.Api.Contracts.Vehicles;

public sealed record CreateVehicleRequest(
    string Brand,
    string Model,
    int Year,
    string Color,
    decimal Price);
