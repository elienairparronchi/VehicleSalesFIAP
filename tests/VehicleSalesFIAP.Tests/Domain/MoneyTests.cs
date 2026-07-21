using VehicleSalesFIAP.Domain.Common;
using VehicleSalesFIAP.Domain.ValueObjects;

namespace VehicleSalesFIAP.Tests.Domain;

public sealed class MoneyTests
{
    [Fact]
    public void MoneyNormalizesCurrencyAndRoundsAmount()
    {
        var money = new Money(123.456m, "brl");

        Assert.Equal(123.46m, money.Amount);
        Assert.Equal("BRL", money.Currency);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void MoneyRejectsNonPositiveAmount(decimal amount)
    {
        Assert.Throws<DomainException>(() => Money.From(amount));
    }
}
