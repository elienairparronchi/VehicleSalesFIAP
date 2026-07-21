using VehicleSalesFIAP.Domain.Common;
using VehicleSalesFIAP.Domain.ValueObjects;

namespace VehicleSalesFIAP.Domain.Sales;

public sealed class Sale
{
    private Sale()
    {
        PurchasePrice = null!;
    }

    private Sale(Guid id, Guid vehicleId, string buyerId, Money purchasePrice, DateTimeOffset purchasedAt)
    {
        Id = id;
        VehicleId = vehicleId;
        BuyerId = NormalizeBuyerId(buyerId);
        PurchasePrice = purchasePrice ?? throw new DomainException("The purchase price is required.");
        PurchasedAt = purchasedAt;
    }

    public Guid Id { get; private set; }

    public Guid VehicleId { get; private set; }

    public string BuyerId { get; private set; } = string.Empty;

    public Money PurchasePrice { get; private set; }

    public DateTimeOffset PurchasedAt { get; private set; }

    internal static Sale Create(Guid vehicleId, string buyerId, Money purchasePrice, DateTimeOffset purchasedAt)
    {
        if (vehicleId == Guid.Empty)
        {
            throw new DomainException("The sale must reference a valid vehicle.");
        }

        return new Sale(Guid.NewGuid(), vehicleId, buyerId, purchasePrice, purchasedAt);
    }

    private static string NormalizeBuyerId(string buyerId)
    {
        if (string.IsNullOrWhiteSpace(buyerId))
        {
            throw new DomainException("The buyer identity is required.");
        }

        var normalized = buyerId.Trim();
        if (normalized.Length > 200)
        {
            throw new DomainException("The buyer identity must have at most 200 characters.");
        }

        return normalized;
    }
}
