using FuelWallet.Application.Tests.Common;
using FuelWallet.Domain.Entities;
using FuelWallet.Domain.Enums;
using FuelWallet.Infrastructure.BackgroundJobs;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FuelWallet.Application.Tests.BackgroundJobs;

public class TransactionExpiryServiceTests
{
    private readonly TestClock _clock = new();

    private TransactionExpiryService CreateService() => new(
        Mock.Of<IServiceScopeFactory>(),                 // unused by the method under test
        NullLogger<TransactionExpiryService>.Instance,
        _clock);

    private FuelTransaction Pending(string reference, TimeSpan ago) => new()
    {
        WalletId = "WLT-TEST", StationId = 1, PumpId = 1, RequestedAmount = 10m,
        Status = TransactionStatus.Pending, RequestReference = reference,
        CreatedAt = _clock.GetUtcNow().UtcDateTime - ago
    };

    [Fact]
    public async Task ExpireStaleTransactions_MarksPendingOlderThanThreshold_AsExpired()
    {
        await using var context = TestDb.Create(_clock);
        context.FuelTransactions.Add(Pending("STALE", TimeSpan.FromMinutes(5))); // > 2 min
        await context.SaveChangesAsync();

        await CreateService().ExpireStaleTransactionsAsync(context, default);

        var tx = await context.FuelTransactions.SingleAsync();
        tx.Status.Should().Be(TransactionStatus.Expired);
        tx.UpdatedAt.Should().Be(_clock.GetUtcNow().UtcDateTime); // interceptor stamped the change
    }

    [Fact]
    public async Task ExpireStaleTransactions_LeavesFreshPending_Untouched()
    {
        await using var context = TestDb.Create(_clock);
        context.FuelTransactions.Add(Pending("FRESH", TimeSpan.FromSeconds(30))); // < 2 min
        await context.SaveChangesAsync();

        await CreateService().ExpireStaleTransactionsAsync(context, default);

        (await context.FuelTransactions.SingleAsync()).Status.Should().Be(TransactionStatus.Pending);
    }

    [Fact]
    public async Task ExpireStaleTransactions_IgnoresTransactions_NotPending()
    {
        await using var context = TestDb.Create(_clock);
        context.FuelTransactions.Add(new FuelTransaction
        {
            WalletId = "WLT-TEST", StationId = 1, PumpId = 1, RequestedAmount = 10m,
            AuthorizedAmount = 10m, Status = TransactionStatus.Authorized,
            RequestReference = "AUTH-OLD",
            CreatedAt = _clock.GetUtcNow().UtcDateTime - TimeSpan.FromMinutes(10)
        });
        await context.SaveChangesAsync();

        await CreateService().ExpireStaleTransactionsAsync(context, default);

        (await context.FuelTransactions.SingleAsync()).Status.Should().Be(TransactionStatus.Authorized);
    }
}
