using FuelWallet.Infrastructure.Persistence;
using FuelWallet.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace FuelWallet.Application.Tests.Common;

/// <summary>
/// Builds in-memory <see cref="ApplicationDbContext"/> instances wired with the real
/// <see cref="AuditableEntityInterceptor"/> and a fixed clock — so tests exercise the
/// same timestamping behaviour as production, deterministically.
/// </summary>
internal static class TestDb
{
    public static ApplicationDbContext Create(TimeProvider clock, string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .AddInterceptors(new AuditableEntityInterceptor(clock))
            .Options;

        return new ApplicationDbContext(options);
    }
}
