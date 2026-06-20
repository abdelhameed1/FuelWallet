using FuelWallet.Domain.Enums;
using FuelWallet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FuelWallet.Infrastructure.BackgroundJobs;

public class TransactionExpiryService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TransactionExpiryService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(30);
    private readonly TimeSpan _expiryThreshold = TimeSpan.FromMinutes(2);

    public TransactionExpiryService(
        IServiceScopeFactory scopeFactory,
        ILogger<TransactionExpiryService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stopToken)
    {
        while (!stopToken.IsCancellationRequested)
        {
            try
            {
                await ExpireStaleTransactions(stopToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error while expiring stale transactions");
            }

            await Task.Delay(_checkInterval, stopToken);
        }
    }

    private async Task ExpireStaleTransactions(CancellationToken cancellationToken)
    {
        
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var cutoff = DateTime.UtcNow - _expiryThreshold;

        var staleTransactions = await context.FuelTransactions
            .Where(t => t.Status == TransactionStatus.Pending && t.CreatedAt < cutoff)
            .ToListAsync(cancellationToken);

        if (staleTransactions.Count == 0)
            return;

        foreach (var transaction in staleTransactions)
        {
            transaction.Expire();
        }

        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("expired {Count} stale pending transactions", staleTransactions.Count);
    }
}