using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace VehicleSalesFIAP.Infrastructure.Persistence;

public sealed class VehicleSalesDbContextFactory : IDesignTimeDbContextFactory<VehicleSalesDbContext>
{
    private const string DefaultConnectionString =
        "Server=localhost,1433;Database=VehicleSalesFIAP;User Id=sa;Password=VehicleSalesFIAP@12345;TrustServerCertificate=True";

    public VehicleSalesDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<VehicleSalesDbContext>()
            .UseSqlServer(
                DefaultConnectionString,
                sqlOptions => sqlOptions.MigrationsAssembly(typeof(VehicleSalesDbContext).Assembly.FullName))
            .Options;

        return new VehicleSalesDbContext(options);
    }
}
