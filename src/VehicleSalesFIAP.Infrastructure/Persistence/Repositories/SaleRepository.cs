using VehicleSalesFIAP.Application.Abstractions.Persistence;
using VehicleSalesFIAP.Domain.Sales;

namespace VehicleSalesFIAP.Infrastructure.Persistence.Repositories;

internal sealed class SaleRepository(VehicleSalesDbContext dbContext) : ISaleRepository
{
    public async Task AddAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        await dbContext.Sales.AddAsync(sale, cancellationToken);
    }
}
