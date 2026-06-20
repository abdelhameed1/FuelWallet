namespace FuelWallet.Application.Common.Exceptions;

public class ConflictException : Exception
{
    public ConflictException(string name, object key)
        : base($"{name} '{key}' is already taken.") { }
}
