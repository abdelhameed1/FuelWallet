using FuelWallet.Application.Tests.Common;
using FuelWallet.Domain.Entities;
using FuelWallet.Domain.Exceptions;
using FluentAssertions;

namespace FuelWallet.Application.Tests.Domain;

// Pure domain unit tests — no DB, no async. The wallet aggregate owns the full
// authorization invariant and returns an outcome instead of throwing for business rejections.
public class WalletTests
{
    [Fact]
    public void Authorize_DeductsBalance_AndSucceeds_WhenWithinAllLimits()
    {
        var wallet = new WalletBuilder().WithBalance(500m).WithDailyLimit(300m).Build();

        var result = wallet.Authorize(amount: 150m, alreadySpentToday: 0m);

        result.Succeeded.Should().BeTrue();
        result.FailureReason.Should().BeNull();
        wallet.Balance.Should().Be(350m);
    }

    [Fact]
    public void Authorize_Fails_WithWalletInactive_AndLeavesBalanceUntouched()
    {
        var wallet = new WalletBuilder().WithBalance(500m).Inactive().Build();

        var result = wallet.Authorize(amount: 100m, alreadySpentToday: 0m);

        result.Succeeded.Should().BeFalse();
        result.FailureReason.Should().Be(AuthorizationFailureReason.WalletInactive);
        wallet.Balance.Should().Be(500m);
    }

    [Fact]
    public void Authorize_Fails_WithDailyLimitExceeded_WhenTodaysSpendPlusAmountExceedsLimit()
    {
        var wallet = new WalletBuilder().WithBalance(1000m).WithDailyLimit(100m).Build();

        var result = wallet.Authorize(amount: 50m, alreadySpentToday: 80m); // 130 > 100

        result.Succeeded.Should().BeFalse();
        result.FailureReason.Should().Be(AuthorizationFailureReason.DailyLimitExceeded);
        wallet.Balance.Should().Be(1000m);
    }

    [Fact]
    public void Authorize_Fails_WithInsufficientBalance_WhenAmountExceedsBalance()
    {
        var wallet = new WalletBuilder().WithBalance(50m).WithDailyLimit(300m).Build();

        var result = wallet.Authorize(amount: 200m, alreadySpentToday: 0m);

        result.Succeeded.Should().BeFalse();
        result.FailureReason.Should().Be(AuthorizationFailureReason.InsufficientBalance);
        wallet.Balance.Should().Be(50m);
    }

    [Fact]
    public void Authorize_ChecksDailyLimit_BeforeBalance()
    {
        // Amount is within balance but over the daily limit — daily-limit reason should win.
        var wallet = new WalletBuilder().WithBalance(1000m).WithDailyLimit(100m).Build();

        var result = wallet.Authorize(amount: 200m, alreadySpentToday: 0m);

        result.FailureReason.Should().Be(AuthorizationFailureReason.DailyLimitExceeded);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void Constructor_Throws_WhenInitialBalanceNegative(decimal initialBalance)
    {
        var act = () => new Wallet("WLT-X", "CUST-1", "Test", "ABC-123",
            initialBalance, dailyLimit: 100m, isActive: true);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_Throws_WhenWalletIdMissing()
    {
        var act = () => new Wallet("", "CUST-1", "Test", "ABC-123",
            initialBalance: 100m, dailyLimit: 100m, isActive: true);

        act.Should().Throw<DomainException>();
    }
}
