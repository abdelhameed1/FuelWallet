using FuelWallet.Application.Auth.Commands.Login;
using FuelWallet.Application.Auth.Commands.Register;
using FuelWallet.Application.FuelAuthorizations.Commands.CreateFuelAuthorization;
using FuelWallet.Application.FuelAuthorizations.Queries.GetTransactionById;
using FuelWallet.Application.Wallets.Queries.GetWalletBalance;
using FuelWallet.Application.Wallets.Queries.GetWalletTransactions;
using FuelWallet.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using System.Threading.RateLimiting;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Route all logging through Serilog; sinks and levels come from configuration.
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services));

    builder.AddApplicationServices();
    builder.AddInfrastructureServices();

    var jwtSecret = builder.Configuration["JwtSettings:Secret"]!;
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                ValidAudience = builder.Configuration["JwtSettings:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
            };
        });
    builder.Services.AddAuthorization();

    builder.Services.AddRateLimiter(options =>
    {
        options.AddFixedWindowLimiter("auth", config =>
        {
            config.Window = TimeSpan.FromMinutes(1);
            config.PermitLimit = 10;
            config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            config.QueueLimit = 0;
        });
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Enter: Bearer {your-token}",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT"
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    var app = builder.Build();

    await ApplyMigrationsAsync(app);

    app.UseSerilogRequestLogging();
    app.UseMiddleware<FuelWallet.Web.Middleware.ExceptionHandlingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    // ── Public ──────────────────────────────────────────────────────────────────

    app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

    app.MapPost("/api/auth/register", async (RegisterUserCommand command, MediatR.ISender sender) =>
    {
        var result = await sender.Send(command);
        return Results.Created("/api/auth/register", new { username = result.Username });
    }).RequireRateLimiting("auth");

    app.MapPost("/api/auth/token", async (LoginCommand command, MediatR.ISender sender) =>
    {
        var result = await sender.Send(command);
        return Results.Ok(new { token = result.Token });
    }).RequireRateLimiting("auth");

    // ── Protected ────────────────────────────────────────────────────────────────

    app.MapPost("/api/fuel-authorizations", async (
        CreateFuelAuthorizationCommand command,
        MediatR.ISender sender) =>
    {
        var result = await sender.Send(command);
        return Results.Ok(result);
    }).RequireAuthorization();

    app.MapGet("/api/transactions/{id:int}", async (
        int id,
        MediatR.ISender sender) =>
    {
        var result = await sender.Send(new GetTransactionByIdQuery(id));
        return Results.Ok(result);
    }).RequireAuthorization();

    app.MapGet("/api/wallets/{walletId}/transactions", async (
        string walletId,
        MediatR.ISender sender) =>
    {
        var result = await sender.Send(new GetWalletTransactionsQuery(walletId));
        return Results.Ok(result);
    }).RequireAuthorization();

    app.MapGet("/api/wallets/{walletId}/balance", async (
        string walletId,
        MediatR.ISender sender) =>
    {
        var result = await sender.Send(new GetWalletBalanceQuery(walletId));
        return Results.Ok(result);
    }).RequireAuthorization();

    // ── Dev only ─────────────────────────────────────────────────────────────────

    if (app.Environment.IsDevelopment())
    {
        app.MapPost("/api/dev/reseed-wallets", async (ApplicationDbContext context, TimeProvider timeProvider) =>
        {
            var seedBalances = new Dictionary<string, decimal>
            {
                ["WLT-1001"] = 500m,
                ["WLT-1002"] = 50m,
                ["WLT-1003"] = 1000m,
                ["WLT-1004"] = 500m,
            };

            var walletIds = seedBalances.Keys.ToList();
            var today = timeProvider.GetUtcNow().UtcDateTime.Date;

            await context.FuelTransactions
                .Where(t => walletIds.Contains(t.WalletId) && t.CreatedAt >= today)
                .ExecuteDeleteAsync();

            foreach (var (walletId, balance) in seedBalances)
            {
                await context.Wallets
                    .Where(w => w.WalletId == walletId)
                    .ExecuteUpdateAsync(s => s.SetProperty(w => w.Balance, balance));
            }

            return Results.Ok(new { message = "Wallet balances and today's transactions reset to seed values.", wallets = seedBalances });
        }).RequireAuthorization();
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly during startup.");
}
finally
{
    Log.CloseAndFlush();
}

static async Task ApplyMigrationsAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (!db.Database.IsRelational())
        return;

    const int maxAttempts = 12;
    for (var attempt = 1; ; attempt++)
    {
        try
        {
            await db.Database.MigrateAsync();
            app.Logger.LogInformation("Database migrations applied.");
            return;
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            app.Logger.LogWarning(ex,
                "Database not ready (attempt {Attempt}/{Max}); retrying in 5s.", attempt, maxAttempts);
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}
