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
        Assert.Equal(vehicle.Price, sale.PurchasePrice);
        Assert.NotSame(vehicle.Price, sale.PurchasePrice);
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

    [Theory]
    [InlineData(1885)]
    [InlineData(2028)]
    public void RegisterForSaleRejectsYearOutsideRangeForReferenceDate(int year)
    {
        var referenceDate = new DateTimeOffset(2026, 7, 20, 12, 0, 0, TimeSpan.Zero);

        Assert.Throws<DomainException>(() => Vehicle.RegisterForSale(
            "Toyota",
            "Corolla",
            year,
            "Silver",
            Money.From(95000),
            referenceDate));
    }

    [Fact]
    public void UpdateDetailsRejectsSoldVehicle()
    {
        var timestamp = new DateTimeOffset(2026, 7, 20, 12, 0, 0, TimeSpan.Zero);
        var vehicle = Vehicle.RegisterForSale(
            "Honda",
            "Civic",
            2021,
            "Black",
            Money.From(110000),
            timestamp.AddDays(-1));
        vehicle.SellTo("buyer-123", timestamp);

        Assert.Throws<DomainException>(() => vehicle.UpdateDetails(
            "Honda",
            "Civic Touring",
            2022,
            "White",
            Money.From(120000),
            timestamp.AddHours(1)));
    }
}
