using FuelWallet.Application.Auth.Commands.Login;
using FuelWallet.Application.Common.Exceptions;
using FuelWallet.Application.Tests.Common;
using FuelWallet.Domain.Entities;
using FluentAssertions;

namespace FuelWallet.Application.Tests.Auth;

public class LoginCommandHandlerTests
{
    private readonly TestClock _clock = new();
    private readonly FakePasswordHasher _hasher = new();
    private readonly StubTokenService _tokens = new();
    private readonly string _db = Guid.NewGuid().ToString(); // isolated store per test instance

    private async Task SeedUser(string username, string password)
    {
        await using var context = TestDb.Create(_clock, _db);
        context.Users.Add(new User { Username = username, PasswordHash = _hasher.Hash(password) });
        await context.SaveChangesAsync();
    }

    private LoginCommandHandler Handler() =>
        new(TestDb.Create(_clock, _db), _hasher, _tokens);

    [Fact]
    public async Task Handle_ReturnsToken_WhenCredentialsValid()
    {
        await SeedUser("station-api", "secret");

        var result = await Handler().Handle(new LoginCommand("station-api", "secret"), default);

        result.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_Throws_WhenPasswordWrong()
    {
        await SeedUser("station-api", "secret");

        var act = () => Handler().Handle(new LoginCommand("station-api", "wrong"), default);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_Throws_WhenUserDoesNotExist()
    {
        var act = () => Handler().Handle(new LoginCommand("ghost", "secret"), default);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }
}
