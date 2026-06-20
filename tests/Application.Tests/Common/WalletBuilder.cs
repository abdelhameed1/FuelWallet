using FuelWallet.Domain.Entities;

namespace FuelWallet.Application.Tests.Common;

/// <summary>
/// Test-data builder for <see cref="Wallet"/>. Hides the 7 positional constructor args
/// behind named, defaulted setters so each test states only the field it cares about.
/// </summary>
internal sealed class WalletBuilder
{
    private string _walletId = "WLT-TEST";
    private decimal _balance = 500m;
    private decimal _dailyLimit = 300m;
    private bool _active = true;

    public WalletBuilder WithId(string id) { _walletId = id; return this; }
    public WalletBuilder WithBalance(decimal balance) { _balance = balance; return this; }
    public WalletBuilder WithDailyLimit(decimal limit) { _dailyLimit = limit; return this; }
    public WalletBuilder Inactive() { _active = false; return this; }

    public Wallet Build() =>
        new(_walletId, "CUST-1", "Test User", "ABC-123", _balance, _dailyLimit, _active);
}
