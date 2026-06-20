namespace FuelWallet.Application.Common.Exceptions;

public class UnauthorizedException : Exception
{
    public UnauthorizedException() : base("Invalid credentials.") { }
}
