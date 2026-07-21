using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehicleSalesFIAP.Domain.Vehicles;

namespace VehicleSalesFIAP.Infrastructure.Persistence.Configurations;

internal sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles");

        builder.HasKey(vehicle => vehicle.Id);

        builder.Property(vehicle => vehicle.Id)
            .ValueGeneratedNever();

        builder.Property(vehicle => vehicle.Brand)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(vehicle => vehicle.Model)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(vehicle => vehicle.Year)
            .IsRequired();

        builder.Property(vehicle => vehicle.Color)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(vehicle => vehicle.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(vehicle => vehicle.CreatedAt)
            .IsRequired();

        builder.Property(vehicle => vehicle.UpdatedAt);

        builder.Property(vehicle => vehicle.SoldAt);

        builder.Property<byte[]>("RowVersion")
            .IsRowVersion()
            .IsRequired();

        builder.OwnsOne(vehicle => vehicle.Price, priceBuilder =>
        {
            priceBuilder.Property(price => price.Amount)
                .HasColumnName("PriceAmount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            priceBuilder.Property(price => price.Currency)
                .HasColumnName("PriceCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Navigation(vehicle => vehicle.Price)
            .IsRequired();

        builder.HasIndex(vehicle => vehicle.Status);
    }
}
