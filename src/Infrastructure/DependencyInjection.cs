using FuelWallet.Application.Common.Interfaces;
using FuelWallet.Infrastructure.BackgroundJobs;
using FuelWallet.Infrastructure.Persistence;
using FuelWallet.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        builder.Services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        builder.Services.AddScoped<IOptimisticConcurrencyExecutor, OptimisticConcurrencyExecutor>();

        builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        builder.Services.AddSingleton<ITokenService, JwtTokenService>();

        builder.Services.AddHostedService<TransactionExpiryService>();
    }
}