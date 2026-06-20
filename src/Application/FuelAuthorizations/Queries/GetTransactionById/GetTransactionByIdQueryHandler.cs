using FuelWallet.Application.Common.Exceptions;
using FuelWallet.Application.Common.Interfaces;
using FuelWallet.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FuelWallet.Application.FuelAuthorizations.Queries.GetTransactionById;

public class GetTransactionByIdQueryHandler : IRequestHandler<GetTransactionByIdQuery, TransactionDto>
{
    private readonly IApplicationDbContext _context;

    public GetTransactionByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TransactionDto> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
    {
        var transaction = await _context.FuelTransactions
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (transaction == null)
            throw new NotFoundException("Transaction", request.Id);

        return new TransactionDto
        {
            Id = transaction.Id,
            WalletId = transaction.WalletId,
            StationId = transaction.StationId,
            PumpId = transaction.PumpId,
            RequestedAmount = transaction.RequestedAmount,
            AuthorizedAmount = transaction.AuthorizedAmount,
            Status = transaction.Status.ToString(),
            RejectionReason = transaction.RejectionReason,
            RequestReference = transaction.RequestReference,
            CreatedAt = transaction.CreatedAt
        };
    }
}