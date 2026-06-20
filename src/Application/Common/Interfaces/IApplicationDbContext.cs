using FuelWallet.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FuelWallet.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Wallet> Wallets { get; }
    DbSet<FuelTransaction> FuelTransactions { get; }
    DbSet<User> Users { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}