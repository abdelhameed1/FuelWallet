using FuelWallet.Application.Auth.Commands.Register;
using FuelWallet.Application.Common.Exceptions;
using FuelWallet.Application.Tests.Common;
using FuelWallet.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FuelWallet.Application.Tests.Auth;

public class RegisterUserCommandHandlerTests
{
    private readonly TestClock _clock = new();
    private readonly FakePasswordHasher _hasher = new();
    private readonly string _db = Guid.NewGuid().ToString();

    [Fact]
    public async Task Handle_CreatesUser_WithHashedPassword()
    {
        await using var context = TestDb.Create(_clock, _db);
        var handler = new RegisterUserCommandHandler(context, _hasher);

        var result = await handler.Handle(new RegisterUserCommand("new-user", "pa55word"), default);

        result.Username.Should().Be("new-user");

        var stored = await context.Users.SingleAsync(u => u.Username == "new-user");
        stored.PasswordHash.Should().Be(_hasher.Hash("pa55word"));
        stored.PasswordHash.Should().NotBe("pa55word"); // never store plaintext
        stored.CreatedAt.Should().Be(_clock.GetUtcNow().UtcDateTime); // stamped by interceptor
    }

    [Fact]
    public async Task Handle_Throws_WhenUsernameAlreadyExists()
    {
        await using (var seed = TestDb.Create(_clock, _db))
        {
            seed.Users.Add(new User { Username = "taken", PasswordHash = _hasher.Hash("x") });
            await seed.SaveChangesAsync();
        }

        await using var context = TestDb.Create(_clock, _db);
        var handler = new RegisterUserCommandHandler(context, _hasher);

        var act = () => handler.Handle(new RegisterUserCommand("taken", "whatever"), default);

        await act.Should().ThrowAsync<ConflictException>();
    }
}
