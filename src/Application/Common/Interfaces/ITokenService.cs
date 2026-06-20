namespace FuelWallet.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateToken(int userId, string username);
}
