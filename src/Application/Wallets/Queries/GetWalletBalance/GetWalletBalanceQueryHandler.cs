using FuelWallet.Application.Common.Exceptions;
using FuelWallet.Application.Common.Interfaces;
using FuelWallet.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FuelWallet.Application.Wallets.Queries.GetWalletBalance;

public class GetWalletBalanceQueryHandler : IRequestHandler<GetWalletBalanceQuery, WalletBalanceDto>
{
    private readonly IApplicationDbContext _context;

    public GetWalletBalanceQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<WalletBalanceDto> Handle(GetWalletBalanceQuery request, CancellationToken cancellationToken)
    {
        var wallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.WalletId == request.WalletId, cancellationToken);

        if (wallet == null)
            throw new NotFoundException(nameof(Wallet), request.WalletId);

        return new WalletBalanceDto
        {
            WalletId = wallet.WalletId,
            Balance = wallet.Balance,
            DailyLimit = wallet.DailyLimit,
            IsActive = wallet.IsActive
        };
    }
}