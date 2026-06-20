using MediatR;

namespace FuelWallet.Application.FuelAuthorizations.Queries.GetTransactionById;

public record GetTransactionByIdQuery(int Id) : IRequest<TransactionDto>;

public record TransactionDto
{
    public int Id { get; init; }
    public string WalletId { get; init; } = default!;
    public int StationId { get; init; }
    public int PumpId { get; init; }
    public decimal RequestedAmount { get; init; }
    public decimal? AuthorizedAmount { get; init; }
    public string Status { get; init; } = default!;
    public string? RejectionReason { get; init; }
    public string RequestReference { get; init; } = default!;
    public DateTime CreatedAt { get; init; }
}