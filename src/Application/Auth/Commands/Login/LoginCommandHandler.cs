using FuelWallet.Application.Common.Exceptions;
using FuelWallet.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FuelWallet.Application.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);

        if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException();

        var token = _tokenService.GenerateToken(user.Id, user.Username);
        return new LoginResult(token);
    }
}
