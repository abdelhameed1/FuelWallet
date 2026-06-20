using FuelWallet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FuelWallet.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(u => u.Username)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(256);

        // Default seed user so the API is usable immediately after migration.
        // Credentials: station-api / P@ssw0rd@123!  (hash is a precomputed BCrypt of that password)
        builder.HasData(new User
        {
            Id = 1,
            Username = "station-api",
            PasswordHash = "$2a$11$lzaqMxWYuz.RUodv.jkjVeOItX/ZbbLbgafajVghbfzEt1BvRAZh6",
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
