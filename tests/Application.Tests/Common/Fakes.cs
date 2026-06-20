using FuelWallet.Application.Common.Interfaces;

namespace FuelWallet.Application.Tests.Common;

/// <summary>Deterministic, fast password hasher for auth tests (no real BCrypt cost).</summary>
internal sealed class FakePasswordHasher : IPasswordHasher
{
    public string Hash(string password) => "hashed:" + password;
    public bool Verify(string password, string hash) => hash == "hashed:" + password;
}

/// <summary>Token service stub that returns a predictable token.</summary>
internal sealed class StubTokenService : ITokenService
{
    public string GenerateToken(int userId, string username) => $"token-{userId}-{username}";
}
