using MediatR;

namespace FuelWallet.Application.Wallets.Queries.GetWalletBalance;

public record GetWalletBalanceQuery(string WalletId) : IRequest<WalletBalanceDto>;

public record WalletBalanceDto
{
    public string WalletId { get; init; } = default!;
    public decimal Balance { get; init; }
    public decimal DailyLimit { get; init; }
    public bool IsActive { get; init; }
}