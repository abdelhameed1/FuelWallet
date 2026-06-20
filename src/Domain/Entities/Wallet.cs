using FuelWallet.Domain.Exceptions;

namespace FuelWallet.Domain.Entities;

public class Wallet : BaseEntity
{
    public string WalletId { get; set; } = default!;
    public string CustomerId { get; set; } = default!;
    public string CustomerName { get; set; } = default!;
    public string VehiclePlate { get; set; } = default!;
    public decimal Balance { get; private set; }
    public decimal DailyLimit { get; set; }
    public bool IsActive { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // Parameterless constructor — required by EF Core itself to materialize entities from the DB
    private Wallet() { }

    // Public constructor — the ONLY other way to set an initial Balance
    public Wallet(string walletId, string customerId, string customerName,
        string vehiclePlate, decimal initialBalance, decimal dailyLimit, bool isActive)
    {
        WalletId = walletId;
        CustomerId = customerId;
        CustomerName = customerName;
        VehiclePlate = vehiclePlate;
        Balance = initialBalance;
        DailyLimit = dailyLimit;
        IsActive = isActive;
    }

    public void EnsureActive()
    {
        if (!IsActive)
            throw new WalletInactiveException();
    }

    public void EnsureSufficientBalance(decimal amount)
    {
        if (amount > Balance)
            throw new InsufficientBalanceException();
    }

    public void EnsureWithinDailyLimit(decimal amount, decimal alreadySpentToday)
    {
        if (alreadySpentToday + amount > DailyLimit)
            throw new DailyLimitExceededException();
    }

    public void Deduct(decimal amount)
    {
        EnsureSufficientBalance(amount);
        Balance -= amount;
    }
}