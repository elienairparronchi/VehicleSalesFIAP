using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehicleSalesFIAP.Domain.Sales;
using VehicleSalesFIAP.Domain.Vehicles;

namespace VehicleSalesFIAP.Infrastructure.Persistence.Configurations;

internal sealed class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");

        builder.HasKey(sale => sale.Id);

        builder.Property(sale => sale.Id)
            .ValueGeneratedNever();

        builder.Property(sale => sale.VehicleId)
            .IsRequired();

        builder.Property(sale => sale.BuyerId)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(sale => sale.PurchasedAt)
            .IsRequired();

        builder.OwnsOne(sale => sale.PurchasePrice, priceBuilder =>
        {
            priceBuilder.Property(price => price.Amount)
                .HasColumnName("PurchasePriceAmount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            priceBuilder.Property(price => price.Currency)
                .HasColumnName("PurchasePriceCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Navigation(sale => sale.PurchasePrice)
            .IsRequired();

        builder.HasOne<Vehicle>()
            .WithMany()
            .HasForeignKey(sale => sale.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(sale => sale.VehicleId)
            .IsUnique();

        builder.HasIndex(sale => sale.BuyerId);
    }
}
