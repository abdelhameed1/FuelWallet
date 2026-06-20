using MediatR;

namespace FuelWallet.Application.FuelAuthorizations.Commands.CreateFuelAuthorization;

public record CreateFuelAuthorizationCommand : IRequest<FuelAuthorizationResult>
{
    public int StationId { get; init; }
    public int PumpId { get; init; }
    public string WalletId { get; init; } = default!;
    public decimal RequestedAmount { get; init; }
    public string RequestReference { get; init; } = default!;
}

public record FuelAuthorizationResult
{
    public int TransactionId { get; init; }
    public string Status { get; init; } = default!;
    public decimal? AuthorizedAmount { get; init; }
    public string? RejectionReason { get; init; }
}