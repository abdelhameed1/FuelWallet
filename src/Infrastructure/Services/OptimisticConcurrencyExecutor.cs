using FuelWallet.Application.Common.Exceptions;
using FuelWallet.Application.Common.Interfaces;
using FuelWallet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FuelWallet.Infrastructure.Services;

public class OptimisticConcurrencyExecutor : IOptimisticConcurrencyExecutor
{
    private const int MaxRetries = 1;
    private readonly ApplicationDbContext _context;

    public OptimisticConcurrencyExecutor(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        Func<CancellationToken, Task<T>> onConflictExhausted,
        CancellationToken cancellationToken)
    {
        var attempt = 0;

        while (true)
        {
            try
            {
                return await operation(cancellationToken);
            }
            catch (ConcurrencyConflictException)
            {
                await ResetTrackedStateAsync(cancellationToken);

                if (attempt++ >= MaxRetries)
                    return await onConflictExhausted(cancellationToken);
            }
        }
    }

    // After a concurrency conflict the context holds stale tracked state: the wallet
    // carries an uncommitted deduction and the new transaction is a pending insert.
    // Reload modified/deleted entities from the database and detach pending inserts so
    // the next attempt (or the fallback) starts from a clean, current snapshot.
    private async Task ResetTrackedStateAsync(CancellationToken cancellationToken)
    {
        foreach (var entry in _context.ChangeTracker.Entries().ToList())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.State = EntityState.Detached;
                    break;
                case EntityState.Modified:
                case EntityState.Deleted:
                    await entry.ReloadAsync(cancellationToken);
                    break;
            }
        }
    }
}
