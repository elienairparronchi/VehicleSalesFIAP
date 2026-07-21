using VehicleSalesFIAP.Domain.Sales;

namespace VehicleSalesFIAP.Application.Vehicles.PurchaseVehicle;

public sealed record PurchaseVehicleResponse(
    Guid SaleId,
    Guid VehicleId,
    string BuyerId,
    decimal PurchasePrice,
    string Currency,
    DateTimeOffset PurchasedAt)
{
    public static PurchaseVehicleResponse FromSale(Sale sale)
    {
        return new PurchaseVehicleResponse(
            sale.Id,
            sale.VehicleId,
            sale.BuyerId,
            sale.PurchasePrice.Amount,
            sale.PurchasePrice.Currency,
            sale.PurchasedAt);
    }
}
