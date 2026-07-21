namespace VehicleSalesFIAP.Application.Vehicles.RegisterVehicleForSale;

public sealed record RegisterVehicleForSaleCommand(
    string Brand,
    string Model,
    int Year,
    string Color,
    decimal Price);
