namespace FuelWallet.Application.Common.Exceptions;

public class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException(Exception inner)
        : base("A concurrency conflict occurred. Please retry.", inner) { }
}
