using FuelWallet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FuelWallet.Infrastructure.Persistence.Configurations;

public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.WalletId)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(w => w.WalletId)
            .IsUnique();

        builder.Property(w => w.CustomerName).HasMaxLength(200);
        builder.Property(w => w.VehiclePlate).HasMaxLength(20);

        builder.Property(w => w.Balance).HasColumnType("decimal(18,2)");
        builder.Property(w => w.DailyLimit).HasColumnType("decimal(18,2)");

        builder.Property(w => w.RowVersion)
            .IsRowVersion();

      builder.HasData(
    new Wallet("WLT-1001", "CUST-001", "Ahmed Hassan", "ABC-1234", 500.00m, 300.00m, true)
    {
        Id = 1,
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    },
    new Wallet("WLT-1002", "CUST-002", "Sara Mostafa", "XYZ-5678", 50.00m, 200.00m, true)
    {
        Id = 2,
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    },
    new Wallet("WLT-1003", "CUST-003", "Omar Khalil", "DEF-9999", 1000.00m, 100.00m, true)
    {
        Id = 3,
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    },
    new Wallet("WLT-1004", "CUST-004", "Layla Ibrahim", "GHI-4321", 500.00m, 300.00m, false)
    {
        Id = 4,
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    }
);
       
    }
}