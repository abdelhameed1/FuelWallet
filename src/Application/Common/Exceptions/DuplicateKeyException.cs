namespace FuelWallet.Application.Common.Exceptions;

/// <summary>
/// Thrown when a write violates a unique constraint/index — i.e. a record with the
/// same unique key already exists. Surfaces a race where a concurrent request inserted
/// the same key first, so callers can recover gracefully instead of returning a 500.
/// </summary>
public class DuplicateKeyException : Exception
{
    public DuplicateKeyException(Exception inner)
        : base("A record with the same unique key already exists.", inner) { }
}
