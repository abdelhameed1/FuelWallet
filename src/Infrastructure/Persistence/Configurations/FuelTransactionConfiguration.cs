using FuelWallet.Domain.Entities;
using FuelWallet.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FuelWallet.Infrastructure.Persistence.Configurations;

public class FuelTransactionConfiguration : IEntityTypeConfiguration<FuelTransaction>
{
    public void Configure(EntityTypeBuilder<FuelTransaction> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.WalletId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.RequestReference)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(t => t.RequestReference)
            .IsUnique();

        builder.Property(t => t.RequestedAmount).HasColumnType("decimal(18,2)");
        builder.Property(t => t.AuthorizedAmount).HasColumnType("decimal(18,2)");

        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.RejectionReason).HasMaxLength(500);

        builder.HasData(
            // WLT-1001 — Ahmed Hassan, Balance: 500, DailyLimit: 300
            new FuelTransaction
            {
                Id = 1, WalletId = "WLT-1001", StationId = 101, PumpId = 1,
                RequestedAmount = 100, AuthorizedAmount = 100,
                Status = TransactionStatus.Authorized,
                RequestReference = "SEED-REF-001",
                CreatedAt = new DateTime(2026, 6, 18, 8, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 6, 18, 8, 0, 1, DateTimeKind.Utc)
            },
            new FuelTransaction
            {
                Id = 2, WalletId = "WLT-1001", StationId = 102, PumpId = 2,
                RequestedAmount = 200, AuthorizedAmount = 200,
                Status = TransactionStatus.Authorized,
                RequestReference = "SEED-REF-002",
                CreatedAt = new DateTime(2026, 6, 17, 10, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 6, 17, 10, 0, 1, DateTimeKind.Utc)
            },
            // WLT-1002 — Sara Mostafa, Balance: 50 (low) — rejected due to insufficient funds
            new FuelTransaction
            {
                Id = 3, WalletId = "WLT-1002", StationId = 101, PumpId = 3,
                RequestedAmount = 300,
                Status = TransactionStatus.Rejected,
                RejectionReason = "Wallet balance is insufficient for this transaction.",
                RequestReference = "SEED-REF-003",
                CreatedAt = new DateTime(2026, 6, 19, 9, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 6, 19, 9, 0, 0, DateTimeKind.Utc)
            },
            // WLT-1003 — Omar Khalil, Balance: 1000, DailyLimit: 100 — shows expired status
            new FuelTransaction
            {
                Id = 4, WalletId = "WLT-1003", StationId = 103, PumpId = 1,
                RequestedAmount = 80, AuthorizedAmount = 80,
                Status = TransactionStatus.Authorized,
                RequestReference = "SEED-REF-004",
                CreatedAt = new DateTime(2026, 6, 16, 14, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 6, 16, 14, 0, 1, DateTimeKind.Utc)
            },
            new FuelTransaction
            {
                Id = 5, WalletId = "WLT-1003", StationId = 101, PumpId = 2,
                RequestedAmount = 50,
                Status = TransactionStatus.Expired,
                RequestReference = "SEED-REF-005",
                CreatedAt = new DateTime(2026, 6, 15, 11, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 6, 15, 11, 2, 0, DateTimeKind.Utc)
            }
        );
    }
}
