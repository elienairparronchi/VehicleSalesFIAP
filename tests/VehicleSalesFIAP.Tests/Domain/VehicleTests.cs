using VehicleSalesFIAP.Domain.Common;
using VehicleSalesFIAP.Domain.ValueObjects;
using VehicleSalesFIAP.Domain.Vehicles;

namespace VehicleSalesFIAP.Tests.Domain;

public sealed class VehicleTests
{
    [Fact]
    public void RegisterForSaleCreatesAvailableVehicle()
    {
        var vehicle = Vehicle.RegisterForSale(
            "Toyota",
            "Corolla",
            2022,
            "Silver",
            Money.From(95000),
            DateTimeOffset.UtcNow);

        Assert.NotEqual(Guid.Empty, vehicle.Id);
        Assert.Equal(VehicleStatus.Available, vehicle.Status);
        Assert.True(vehicle.IsAvailable);
        Assert.Equal(95000, vehicle.Price.Amount);
        Assert.Equal(Money.DefaultCurrency, vehicle.Price.Currency);
    }

    [Fact]
    public void SellToMarksVehicleAsSoldAndCreatesSaleWithCurrentPrice()
    {
        var soldAt = DateTimeOffset.UtcNow;
        var vehicle = Vehicle.RegisterForSale(
            "Honda",
            "Civic",
            2021,
            "Black",
            Money.From(110000),
            soldAt.AddDays(-1));

        var sale = vehicle.SellTo("keycloak-user-123", soldAt);

        Assert.Equal(VehicleStatus.Sold, vehicle.Status);
        Assert.False(vehicle.IsAvailable);
        Assert.Equal(soldAt, vehicle.SoldAt);
        Assert.Equal(vehicle.Id, sale.VehicleId);
        Assert.Equal("keycloak-user-123", sale.BuyerId);
        Assert.Equal(110000, sale.PurchasePrice.Amount);
    }

    [Fact]
    public void SellToRejectsVehicleAlreadySold()
    {
        var vehicle = Vehicle.RegisterForSale(
            "Ford",
            "Ka",
            2020,
            "White",
            Money.From(45000),
            DateTimeOffset.UtcNow.AddDays(-2));

        vehicle.SellTo("buyer-a", DateTimeOffset.UtcNow.AddDays(-1));

        Assert.Throws<DomainException>(() => vehicle.SellTo("buyer-b", DateTimeOffset.UtcNow));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void RegisterForSaleRejectsInvalidBrand(string brand)
    {
        Assert.Throws<DomainException>(() => Vehicle.RegisterForSale(
            brand,
            "Model",
            2022,
            "Blue",
            Money.From(50000),
            DateTimeOffset.UtcNow));
    }
}
