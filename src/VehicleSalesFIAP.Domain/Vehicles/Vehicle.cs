using VehicleSalesFIAP.Domain.Common;
using VehicleSalesFIAP.Domain.Sales;
using VehicleSalesFIAP.Domain.ValueObjects;

namespace VehicleSalesFIAP.Domain.Vehicles;

public sealed class Vehicle
{
    public const int FirstAutomobileYear = 1886;

    private Vehicle()
    {
        Price = null!;
    }

    private Vehicle(Guid id, string brand, string model, int year, string color, Money price, DateTimeOffset createdAt)
    {
        Id = id;
        Brand = NormalizeRequiredText(brand, nameof(brand), 100);
        Model = NormalizeRequiredText(model, nameof(model), 100);
        Year = ValidateYear(year, createdAt);
        Color = NormalizeRequiredText(color, nameof(color), 50);
        Price = price ?? throw new DomainException("The vehicle price is required.");
        Status = VehicleStatus.Available;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public string Brand { get; private set; } = string.Empty;

    public string Model { get; private set; } = string.Empty;

    public int Year { get; private set; }

    public string Color { get; private set; } = string.Empty;

    public Money Price { get; private set; }

    public VehicleStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public DateTimeOffset? SoldAt { get; private set; }

    public bool IsAvailable => Status == VehicleStatus.Available;

    public static Vehicle RegisterForSale(
        string brand,
        string model,
        int year,
        string color,
        Money price,
        DateTimeOffset createdAt)
    {
        return new Vehicle(Guid.NewGuid(), brand, model, year, color, price, createdAt);
    }

    public void UpdateDetails(string brand, string model, int year, string color, Money price, DateTimeOffset updatedAt)
    {
        EnsureAvailable("Sold vehicles cannot be edited.");

        Brand = NormalizeRequiredText(brand, nameof(brand), 100);
        Model = NormalizeRequiredText(model, nameof(model), 100);
        Year = ValidateYear(year, updatedAt);
        Color = NormalizeRequiredText(color, nameof(color), 50);
        Price = price ?? throw new DomainException("The vehicle price is required.");
        UpdatedAt = updatedAt;
    }

    public Sale SellTo(string buyerId, DateTimeOffset soldAt)
    {
        EnsureAvailable("Vehicle is already sold.");

        Status = VehicleStatus.Sold;
        SoldAt = soldAt;
        UpdatedAt = soldAt;

        return Sale.Create(Id, buyerId, Price, soldAt);
    }

    public void EnsureCanBeDeleted()
    {
        EnsureAvailable("Sold vehicles cannot be deleted.");
    }

    private void EnsureAvailable(string message)
    {
        if (!IsAvailable)
        {
            throw new DomainException(message);
        }
    }

    private static string NormalizeRequiredText(string value, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"The vehicle {fieldName} is required.");
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new DomainException($"The vehicle {fieldName} must have at most {maxLength} characters.");
        }

        return normalized;
    }

    private static int ValidateYear(int year, DateTimeOffset referenceDate)
    {
        var maxAllowedYear = referenceDate.Year + 1;
        if (year is < FirstAutomobileYear || year > maxAllowedYear)
        {
            throw new DomainException($"The vehicle year must be between {FirstAutomobileYear} and {maxAllowedYear}.");
        }

        return year;
    }
}
