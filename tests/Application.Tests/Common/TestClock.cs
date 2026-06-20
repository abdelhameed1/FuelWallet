namespace FuelWallet.Application.Tests.Common;

/// <summary>
/// A <see cref="TimeProvider"/> pinned to a fixed instant so time-dependent logic
/// (daily-limit reset at UTC midnight, transaction expiry) is fully deterministic.
/// </summary>
internal sealed class TestClock : TimeProvider
{
    // Default "now" used across the suite unless a test sets its own.
    public static readonly DateTimeOffset Default =
        new(2026, 6, 21, 10, 0, 0, TimeSpan.Zero);

    private DateTimeOffset _now;

    public TestClock(DateTimeOffset? now = null) => _now = now ?? Default;

    public override DateTimeOffset GetUtcNow() => _now;

    public void Advance(TimeSpan by) => _now = _now.Add(by);
}
