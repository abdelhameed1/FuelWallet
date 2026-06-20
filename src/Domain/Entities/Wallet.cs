using FuelWallet.Domain.Exceptions;

namespace FuelWallet.Domain.Entities;

public class Wallet : BaseEntity
{
    public string WalletId { get; private set; } = default!;
    public string CustomerId { get; private set; } = default!;
    public string CustomerName { get; private set; } = default!;
    public string VehiclePlate { get; private set; } = default!;
    public decimal Balance { get; private set; }
    public decimal DailyLimit { get; private set; }
    public bool IsActive { get; private set; }
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    // Parameterless constructor — required by EF Core itself to materialize entities from the DB.
    private Wallet() { }

    public Wallet(string walletId, string customerId, string customerName,
        string vehiclePlate, decimal initialBalance, decimal dailyLimit, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(walletId))
            throw new DomainException("Wallet id is required.");
        if (initialBalance < 0)
            throw new DomainException("Initial balance cannot be negative.");
        if (dailyLimit < 0)
            throw new DomainException("Daily limit cannot be negative.");

        WalletId = walletId;
        CustomerId = customerId;
        CustomerName = customerName;
        VehiclePlate = vehiclePlate;
        Balance = initialBalance;
        DailyLimit = dailyLimit;
        IsActive = isActive;
    }

    
    public AuthorizationResult Authorize(decimal amount, decimal alreadySpentToday)
    {
        if (!IsActive)
            return AuthorizationResult.Rejected(
                AuthorizationFailureReason.WalletInactive,
                "Wallet is not active.");

        if (alreadySpentToday + amount > DailyLimit)
            return AuthorizationResult.Rejected(
                AuthorizationFailureReason.DailyLimitExceeded,
                "Requested amount exceeds the wallet's daily limit.");

        if (amount > Balance)
            return AuthorizationResult.Rejected(
                AuthorizationFailureReason.InsufficientBalance,
                "Wallet balance is insufficient for this transaction.");

        Balance -= amount;
        return AuthorizationResult.Success();
    }
}
