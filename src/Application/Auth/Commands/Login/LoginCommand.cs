using MediatR;

namespace FuelWallet.Application.Auth.Commands.Login;

public record LoginCommand(string Username, string Password) : IRequest<LoginResult>;

public record LoginResult(string Token);
