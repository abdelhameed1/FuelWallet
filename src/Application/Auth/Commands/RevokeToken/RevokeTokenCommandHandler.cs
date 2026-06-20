using FuelWallet.Application.Common.Interfaces;
using FuelWallet.Domain.Entities;
using MediatR;

namespace FuelWallet.Application.Auth.Commands.RevokeToken;

public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand>
{
    private readonly IApplicationDbContext _context;

    public RevokeTokenCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        _context.RevokedTokens.Add(new RevokedToken
        {
            Jti = request.Jti,
            ExpiresAt = request.ExpiresAt
        });
        await _context.SaveChangesAsync(cancellationToken);
    }
}
