using FuelWallet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FuelWallet.Infrastructure.Persistence.Configurations;

public class RevokedTokenConfiguration : IEntityTypeConfiguration<RevokedToken>
{
    public void Configure(EntityTypeBuilder<RevokedToken> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Jti).IsRequired().HasMaxLength(36);
        builder.HasIndex(t => t.Jti).IsUnique();
        builder.Property(t => t.ExpiresAt).IsRequired();
    }
}
