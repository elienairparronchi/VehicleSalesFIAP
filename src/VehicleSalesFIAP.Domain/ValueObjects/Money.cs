using VehicleSalesFIAP.Domain.Common;

namespace VehicleSalesFIAP.Domain.ValueObjects;

public sealed class Money : IEquatable<Money>
{
    public const string DefaultCurrency = "BRL";

    private Money()
    {
        Currency = DefaultCurrency;
    }

    public Money(decimal amount, string currency = DefaultCurrency)
    {
        if (amount <= 0)
        {
            throw new DomainException("The monetary amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new DomainException("The currency is required.");
        }

        var normalizedCurrency = currency.Trim().ToUpperInvariant();
        if (normalizedCurrency.Length != 3)
        {
            throw new DomainException("The currency must use the ISO 4217 three-letter code.");
        }

        Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        Currency = normalizedCurrency;
    }

    public decimal Amount { get; private set; }

    public string Currency { get; private set; }

    public static Money From(decimal amount)
    {
        return new Money(amount);
    }

    public bool Equals(Money? other)
    {
        return other is not null
            && Amount == other.Amount
            && Currency == other.Currency;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Money);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Amount, Currency);
    }

    public override string ToString()
    {
        return $"{Currency} {Amount:N2}";
    }
}
