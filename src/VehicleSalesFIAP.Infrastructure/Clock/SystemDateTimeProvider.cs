using VehicleSalesFIAP.Application.Abstractions.Clock;

namespace VehicleSalesFIAP.Infrastructure.Clock;

internal sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
