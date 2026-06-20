using FuelWallet.Domain.Enums;

namespace FuelWallet.Domain.Entities;

public class FuelTransaction : BaseEntity
{
    public string WalletId { get; set; } = default!;
    public int StationId { get; set; }
    public int PumpId { get; set; }
    public decimal RequestedAmount { get; set; }
    public decimal? AuthorizedAmount { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
    public string? RejectionReason { get; set; }
    public string RequestReference { get; set; } = default!;

    public void Authorize(decimal amount)
    {
        AuthorizedAmount = amount;
        Status = TransactionStatus.Authorized;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject(string reason)
    {
        Status = TransactionStatus.Rejected;
        RejectionReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Expire()
    {
        Status = TransactionStatus.Expired;
        UpdatedAt = DateTime.UtcNow;
    }
}