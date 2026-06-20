namespace FuelWallet.Domain.Entities;


public enum AuthorizationFailureReason
{
    WalletInactive,
    InsufficientBalance,
    DailyLimitExceeded
}

public sealed record AuthorizationResult
{
    public bool Succeeded { get; }
    public AuthorizationFailureReason? FailureReason { get; }
    public string? RejectionMessage { get; }

    private AuthorizationResult(bool succeeded, AuthorizationFailureReason? reason, string? message)
    {
        Succeeded = succeeded;
        FailureReason = reason;
        RejectionMessage = message;
    }

    public static AuthorizationResult Success() => new(true, null, null);

    public static AuthorizationResult Rejected(AuthorizationFailureReason reason, string message) =>
        new(false, reason, message);
}
