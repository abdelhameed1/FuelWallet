namespace FuelWallet.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class InsufficientBalanceException : DomainException
{
    public InsufficientBalanceException()
        : base("Wallet balance is insufficient for this transaction.") { }
}

public class WalletInactiveException : DomainException
{
    public WalletInactiveException()
        : base("wallet is not active.") { }
}

public class DailyLimitExceededException : DomainException
{
    public DailyLimitExceededException()
        : base("requested amount exceeds the wallet's daily limit.") { }
}