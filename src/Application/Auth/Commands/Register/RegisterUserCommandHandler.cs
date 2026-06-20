using FuelWallet.Application.Common.Exceptions;
using FuelWallet.Application.Common.Interfaces;
using FuelWallet.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FuelWallet.Application.Auth.Commands.Register;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<RegisterUserResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var exists = await _context.Users.AnyAsync(u => u.Username == request.Username, cancellationToken);
        if (exists)
            throw new ConflictException("Username", request.Username);

        var user = new User
        {
            Username = request.Username,
            PasswordHash = _passwordHasher.Hash(request.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return new RegisterUserResult(user.Username);
    }
}
