using FuelWallet.Application.Common.Exceptions;
using FuelWallet.Application.FuelAuthorizations.Commands.CreateFuelAuthorization;
using FuelWallet.Application.Tests.Common;
using FuelWallet.Domain.Entities;
using FuelWallet.Domain.Enums;
using FuelWallet.Infrastructure.Persistence;
using FuelWallet.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FuelWallet.Application.Tests.FuelAuthorizations;

public class CreateFuelAuthorizationCommandHandlerTests
{
    private readonly TestClock _clock = new();

    private CreateFuelAuthorizationCommandHandler CreateHandler(ApplicationDbContext context) =>
        new(context, new OptimisticConcurrencyExecutor(context), _clock);

    private static CreateFuelAuthorizationCommand Command(
        string walletId = "WLT-TEST",
        decimal requestedAmount = 150m,
        string requestReference = "REQ-001") => new()
        {
            WalletId = walletId,
            StationId = 101,
            PumpId = 1,
            RequestedAmount = requestedAmount,
            RequestReference = requestReference
        };

    [Fact]
    public async Task Handle_AuthorizesAndDeducts_WhenSufficientBalance()
    {
        // arrange
        await using var context = TestDb.Create(_clock);
        context.Wallets.Add(new WalletBuilder().WithBalance(500m).WithDailyLimit(300m).Build());
        await context.SaveChangesAsync();

        // act
        var result = await CreateHandler(context).Handle(Command(requestedAmount: 150m), default);

        // assert
        result.Status.Should().Be("Authorized");
        result.AuthorizedAmount.Should().Be(150m);
        result.TransactionId.Should().BeGreaterThan(0);
        (await context.Wallets.FirstAsync()).Balance.Should().Be(350m);
    }

    [Fact]
    public async Task Handle_Rejects_WithoutDeducting_WhenInsufficientBalance()
    {
        await using var context = TestDb.Create(_clock);
        context.Wallets.Add(new WalletBuilder().WithBalance(50m).Build());
        await context.SaveChangesAsync();

        var result = await CreateHandler(context).Handle(Command(requestedAmount: 200m), default);

        result.Status.Should().Be("Rejected");
        result.AuthorizedAmount.Should().BeNull();
        (await context.Wallets.FirstAsync()).Balance.Should().Be(50m);
    }

    [Fact]
    public async Task Handle_Rejects_WhenWalletInactive()
    {
        await using var context = TestDb.Create(_clock);
        context.Wallets.Add(new WalletBuilder().WithBalance(500m).Inactive().Build());
        await context.SaveChangesAsync();

        var result = await CreateHandler(context).Handle(Command(requestedAmount: 100m), default);

        result.Status.Should().Be("Rejected");
    }

    [Fact]
    public async Task Handle_Rejects_WhenDailyLimitExceeded()
    {
        // arrange — 80 already authorized today against a 100 daily limit
        await using var context = TestDb.Create(_clock);
        context.Wallets.Add(new WalletBuilder().WithBalance(1000m).WithDailyLimit(100m).Build());
        context.FuelTransactions.Add(AuthorizedToday(amount: 80m, reference: "PREV-TODAY"));
        await context.SaveChangesAsync();

        // act — 80 + 50 = 130 > 100
        var result = await CreateHandler(context).Handle(
            Command(requestedAmount: 50m, requestReference: "REQ-LIMIT"), default);

        // assert
        result.Status.Should().Be("Rejected");
        (await context.Wallets.FirstAsync()).Balance.Should().Be(1000m);
    }

    [Fact]
    public async Task Handle_IgnoresYesterdaysSpend_WhenEnforcingTodaysDailyLimit()
    {
        // arrange — 90 authorized YESTERDAY against a 100 daily limit; today's spend is 0
        await using var context = TestDb.Create(_clock);
        context.Wallets.Add(new WalletBuilder().WithBalance(1000m).WithDailyLimit(100m).Build());
        context.FuelTransactions.Add(new FuelTransaction
        {
            WalletId = "WLT-TEST", StationId = 101, PumpId = 1,
            RequestedAmount = 90m, AuthorizedAmount = 90m,
            Status = TransactionStatus.Authorized, RequestReference = "PREV-YDAY",
            CreatedAt = _clock.GetUtcNow().UtcDateTime.AddDays(-1) // backdated; interceptor leaves it
        });
        await context.SaveChangesAsync();

        // act — 80 today is within the 100 limit once yesterday's 90 is excluded
        var result = await CreateHandler(context).Handle(
            Command(requestedAmount: 80m, requestReference: "REQ-TODAY"), default);

        // assert
        result.Status.Should().Be("Authorized");
        (await context.Wallets.FirstAsync()).Balance.Should().Be(920m);
    }

    [Fact]
    public async Task Handle_ReturnsOriginalResult_WhenRequestReferenceIsDuplicate()
    {
        await using var context = TestDb.Create(_clock);
        context.Wallets.Add(new WalletBuilder().WithBalance(500m).Build());
        await context.SaveChangesAsync();

        var handler = CreateHandler(context);
        var command = Command(requestedAmount: 150m, requestReference: "REQ-DUP");

        var first = await handler.Handle(command, default);
        var second = await handler.Handle(command, default);

        // idempotent: same transaction returned, balance deducted exactly once
        second.TransactionId.Should().Be(first.TransactionId);
        second.Status.Should().Be(first.Status);
        (await context.Wallets.FirstAsync()).Balance.Should().Be(350m);
    }

    [Fact]
    public async Task Handle_Throws_WhenWalletNotFound()
    {
        await using var context = TestDb.Create(_clock);
        var handler = CreateHandler(context);

        var act = () => handler.Handle(Command(walletId: "WLT-MISSING"), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    private FuelTransaction AuthorizedToday(decimal amount, string reference) => new()
    {
        WalletId = "WLT-TEST", StationId = 101, PumpId = 1,
        RequestedAmount = amount, AuthorizedAmount = amount,
        Status = TransactionStatus.Authorized, RequestReference = reference,
        CreatedAt = _clock.GetUtcNow().UtcDateTime
    };
}
