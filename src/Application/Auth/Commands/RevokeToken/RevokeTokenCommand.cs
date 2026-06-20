using MediatR;

namespace FuelWallet.Application.Auth.Commands.RevokeToken;

public record RevokeTokenCommand(string Jti, DateTime ExpiresAt) : IRequest;
