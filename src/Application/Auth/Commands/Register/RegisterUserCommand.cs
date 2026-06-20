using MediatR;

namespace FuelWallet.Application.Auth.Commands.Register;

public record RegisterUserCommand(string Username, string Password) : IRequest<RegisterUserResult>;

public record RegisterUserResult(string Username);
