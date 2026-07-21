namespace VehicleSalesFIAP.Application.Vehicles.PurchaseVehicle;

public sealed record PurchaseVehicleCommand(Guid VehicleId, string BuyerId);
