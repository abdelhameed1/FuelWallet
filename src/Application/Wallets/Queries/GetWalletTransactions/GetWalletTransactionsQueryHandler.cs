using FuelWallet.Application.Common.Exceptions;
using FuelWallet.Application.Common.Interfaces;
using FuelWallet.Application.FuelAuthorizations.Queries.GetTransactionById;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FuelWallet.Application.Wallets.Queries.GetWalletTransactions;

public class GetWalletTransactionsQueryHandler
    : IRequestHandler<GetWalletTransactionsQuery, List<TransactionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetWalletTransactionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TransactionDto>> Handle(GetWalletTransactionsQuery request, CancellationToken cancellationToken)
    {
        var walletExists = await _context.Wallets
            .AnyAsync(w => w.WalletId == request.WalletId, cancellationToken);

        if (!walletExists)
            throw new NotFoundException("Wallet", request.WalletId);

        return await _context.FuelTransactions
            .Where(t => t.WalletId == request.WalletId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TransactionDto
            {
                Id = t.Id,
                WalletId = t.WalletId,
                StationId = t.StationId,
                PumpId = t.PumpId,
                RequestedAmount = t.RequestedAmount,
                AuthorizedAmount = t.AuthorizedAmount,
                Status = t.Status.ToString(),
                RejectionReason = t.RejectionReason,
                RequestReference = t.RequestReference,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
