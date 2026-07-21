using VehicleSalesFIAP.Domain.Sales;

namespace VehicleSalesFIAP.Application.Abstractions.Persistence;

public interface ISaleRepository
{
    Task AddAsync(Sale sale, CancellationToken cancellationToken = default);
}
