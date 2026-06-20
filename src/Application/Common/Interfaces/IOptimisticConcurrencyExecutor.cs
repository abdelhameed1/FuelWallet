namespace FuelWallet.Application.Common.Interfaces;

/// <summary>
/// Runs a unit of work and retries it once if the persistence layer reports an
/// optimistic-concurrency conflict. All EF Core change-tracker mechanics (reloading
/// stale entities, discarding pending inserts) live behind this abstraction in the
/// Infrastructure layer — the Application layer stays free of persistence concerns.
/// </summary>
public interface IOptimisticConcurrencyExecutor
{
    /// <param name="operation">The work to perform, including its own SaveChanges.</param>
    /// <param name="onConflictExhausted">
    /// Fallback invoked when retries are exhausted (tracked state has already been reset).
    /// </param>
    Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        Func<CancellationToken, Task<T>> onConflictExhausted,
        CancellationToken cancellationToken);
}
