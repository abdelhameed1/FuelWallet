using FuelWallet.Application.Common.Exceptions;
using FuelWallet.Application.Tests.Common;
using FuelWallet.Infrastructure.Services;
using FluentAssertions;

namespace FuelWallet.Application.Tests.Infrastructure;

// Exercises the retry policy that was moved out of the handler into Infrastructure.
// The operation delegate is faked, so the conflict path is tested without a real database.
public class OptimisticConcurrencyExecutorTests
{
    private readonly TestClock _clock = new();

    private OptimisticConcurrencyExecutor CreateExecutor() =>
        new(TestDb.Create(_clock)); // empty context — ResetTrackedState is a no-op

    [Fact]
    public async Task ExecuteAsync_ReturnsOperationResult_WhenNoConflict()
    {
        var executor = CreateExecutor();
        var attempts = 0;

        var result = await executor.ExecuteAsync(
            operation: _ => { attempts++; return Task.FromResult("ok"); },
            onConflictExhausted: _ => Task.FromResult("fallback"),
            CancellationToken.None);

        result.Should().Be("ok");
        attempts.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_RetriesOnce_ThenSucceeds()
    {
        var executor = CreateExecutor();
        var attempts = 0;

        var result = await executor.ExecuteAsync(
            operation: _ =>
            {
                attempts++;
                if (attempts == 1)
                    throw new ConcurrencyConflictException(new Exception("boom"));
                return Task.FromResult("ok-on-retry");
            },
            onConflictExhausted: _ => Task.FromResult("fallback"),
            CancellationToken.None);

        result.Should().Be("ok-on-retry");
        attempts.Should().Be(2);
    }

    [Fact]
    public async Task ExecuteAsync_RunsConflictFallback_WhenRetriesExhausted()
    {
        var executor = CreateExecutor();
        var attempts = 0;
        var fallbackRan = false;

        var result = await executor.ExecuteAsync(
            operation: _ =>
            {
                attempts++;
                throw new ConcurrencyConflictException(new Exception("always conflicts"));
            },
            onConflictExhausted: _ => { fallbackRan = true; return Task.FromResult("fallback"); },
            CancellationToken.None);

        result.Should().Be("fallback");
        fallbackRan.Should().BeTrue();
        attempts.Should().Be(2); // initial attempt + one retry, then fallback
    }
}
