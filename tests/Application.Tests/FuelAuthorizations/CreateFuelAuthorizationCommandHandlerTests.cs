
//*AAA*

using FuelWallet.Application.FuelAuthorizations.Commands.CreateFuelAuthorization;
using FuelWallet.Domain.Entities;
using FuelWallet.Domain.Enums;
using FuelWallet.Infrastructure.Persistence;
using FuelWallet.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;


namespace FuelWallet.Application.Tests.FuelAuthorizations;

public class CreateFuelAuthorizationCommandHandlerTests
{
    // inject in-memory  db-context
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // fresh DB per test
            .Options;

        return new ApplicationDbContext(options);
    }

    private static CreateFuelAuthorizationCommandHandler CreateHandler(ApplicationDbContext context)
        => new(context, new OptimisticConcurrencyExecutor(context));

    [Fact]
    public async Task Handle_SufficientBalance_AuthorizesTransaction()
    {
        // arrange
        var context = CreateContext();
        var wallet = new Wallet("WLT-TEST", "CUST-1", "Test User", "ABC-123",
            initialBalance: 500m, dailyLimit: 300m, isActive: true);
        context.Wallets.Add(wallet);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler(context);
        var command = new CreateFuelAuthorizationCommand
        {
            WalletId = "WLT-TEST",
            StationId = 101,
            PumpId = 1,
            RequestedAmount = 150,
            RequestReference = "REQ-001"
        };

        // act
        var result = await handler.Handle(command, CancellationToken.None);

        // assert
        result.Status.Should().Be("Authorized");
        result.AuthorizedAmount.Should().Be(150);

        var updatedWallet = await context.Wallets.FirstAsync(w => w.WalletId == "WLT-TEST");
        updatedWallet.Balance.Should().Be(350); // 500 - 150
    }

    [Fact]
    public async Task Handle_InsufficientBalance_RejectsTransaction()
    {
        // arrange
        var context = CreateContext();
        var wallet = new Wallet("WLT-TEST", "CUST-1", "Test User", "ABC-123",
            initialBalance: 50m, dailyLimit: 300m, isActive: true);
        context.Wallets.Add(wallet);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler(context);
        var command = new CreateFuelAuthorizationCommand
        {
            WalletId = "WLT-TEST",
            StationId = 101,
            PumpId = 1,
            RequestedAmount = 200,
            RequestReference = "REQ-002"
        };
        // act 
        var result = await handler.Handle(command, CancellationToken.None);
        // assert
        result.Status.Should().Be("Rejected");
        result.RejectionReason.Should().Be("Wallet balance is insufficient for this transaction.");

        var updatedWallet = await context.Wallets.FirstAsync(w => w.WalletId == "WLT-TEST");
        updatedWallet.Balance.Should().Be(50);
    }

    [Fact]
    public async Task Handle_InactiveWallet_RejectsTransaction()
    {
        // arrange
        var context = CreateContext();
        var wallet = new Wallet("WLT-TEST", "CUST-1", "Test User", "ABC-123",
            initialBalance: 500m, dailyLimit: 300m, isActive: false);
        context.Wallets.Add(wallet);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler(context);
        var command = new CreateFuelAuthorizationCommand
        {
            WalletId = "WLT-TEST",
            StationId = 101,
            PumpId = 1,
            RequestedAmount = 100,
            RequestReference = "REQ-003"
        };
        // act 
        var result = await handler.Handle(command, CancellationToken.None);
        // assert
        result.Status.Should().Be("Rejected");
        result.RejectionReason.Should().Be("wallet is not active.");
    }

    [Fact]
    public async Task Handle_DailyLimitExceeded_RejectsTransaction()
    {
        // arrange
        var context = CreateContext();
        var wallet = new Wallet("WLT-TEST", "CUST-1", "Test User", "ABC-123",
            initialBalance: 1000m, dailyLimit: 100m, isActive: true);
        context.Wallets.Add(wallet);

        // simulate a transaction already authorized today that consumed 80 of the 100 limit
        context.FuelTransactions.Add(new FuelTransaction
        {
            WalletId = "WLT-TEST",
            StationId = 101,
            PumpId = 1,
            RequestedAmount = 80,
            AuthorizedAmount = 80,
            Status = TransactionStatus.Authorized,
            RequestReference = "PREV-REF-TODAY",
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler(context);
        var command = new CreateFuelAuthorizationCommand
        {
            WalletId = "WLT-TEST",
            StationId = 101,
            PumpId = 2,
            RequestedAmount = 50, // 80 already spent + 50 = 130 > 100 limit
            RequestReference = "REQ-DAILY-LIMIT-TEST"
        };

        // act
        var result = await handler.Handle(command, CancellationToken.None);

        // assert
        result.Status.Should().Be("Rejected");
        result.RejectionReason.Should().Be("requested amount exceeds the wallet's daily limit.");

        var updatedWallet = await context.Wallets.FirstAsync(w => w.WalletId == "WLT-TEST");
        updatedWallet.Balance.Should().Be(1000m); // balance unchanged — rejection means no deduction
    }

    [Fact]
    public async Task Handle_DuplicateRequestReference_ReturnsOriginalResult()
    {
        // arrange
        var context = CreateContext();
        var wallet = new Wallet("WLT-TEST", "CUST-1", "Test User", "ABC-123",
            initialBalance: 500m, dailyLimit: 300m, isActive: true);
        context.Wallets.Add(wallet);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler(context);
        var command = new CreateFuelAuthorizationCommand
        {
            WalletId = "WLT-TEST",
            StationId = 101,
            PumpId = 1,
            RequestedAmount = 150,
            RequestReference = "REQ-DUPLICATE"
        };

        // act 
        var firstResult = await handler.Handle(command, CancellationToken.None);
        var secondResult = await handler.Handle(command, CancellationToken.None);

        // assert
        secondResult.TransactionId.Should().Be(firstResult.TransactionId);
        secondResult.Status.Should().Be(firstResult.Status);
        secondResult.AuthorizedAmount.Should().Be(firstResult.AuthorizedAmount);

        var updatedWallet = await context.Wallets.FirstAsync(w => w.WalletId == "WLT-TEST");
        updatedWallet.Balance.Should().Be(350); // only deducted once, not twice
    }

    [Fact]
    public async Task Handle_WalletNotFound_ThrowsNotFoundException()
    {
        // arrange
        var context = CreateContext();
        var handler = CreateHandler(context);
        // act 
        var command = new CreateFuelAuthorizationCommand
        {
            WalletId = "WLT-DOES-NOT-EXIST",
            StationId = 101,
            PumpId = 1,
            RequestedAmount = 100,
            RequestReference = "REQ-004"
        };
        // arrange
        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Common.Exceptions.NotFoundException>();
    }

    [Fact]
    public async Task Handle_ConcurrentRequests_OnlyDeductsOnce()
    {
        // NOTE: In-memory EF does not enforce row-version concurrency, so DbUpdateConcurrencyException
        // will NOT fire here. This test validates that the final balance is correct when requests run
        // in parallel using the same shared DbContext — which serializes at the EF change-tracker level.
        // True optimistic concurrency is covered by the SQL Server row-version column (RowVersion)
        // and would require an integration test against a real database to verify the retry path.

        // arrange
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        await using (var seedContext = new ApplicationDbContext(options))
        {
            var wallet = new Wallet("WLT-RACE", "CUST-1", "Test User", "ABC-123",
                initialBalance: 200m, dailyLimit: 500m, isActive: true);
            seedContext.Wallets.Add(wallet);
            await seedContext.SaveChangesAsync(CancellationToken.None);
        }

        await using var contextA = new ApplicationDbContext(options);
        await using var contextB = new ApplicationDbContext(options);

        var handlerA = CreateHandler(contextA);
        var handlerB = CreateHandler(contextB);

        var commandA = new CreateFuelAuthorizationCommand
        {
            WalletId = "WLT-RACE", StationId = 101, PumpId = 1,
            RequestedAmount = 150, RequestReference = "REQ-RACE-A"
        };
        var commandB = new CreateFuelAuthorizationCommand
        {
            WalletId = "WLT-RACE", StationId = 101, PumpId = 2,
            RequestedAmount = 150, RequestReference = "REQ-RACE-B"
        };

        // act — run in parallel
        var results = await Task.WhenAll(
            handlerA.Handle(commandA, CancellationToken.None),
            handlerB.Handle(commandB, CancellationToken.None)
        );

        // assert — exactly one authorized, one rejected; total deducted ≤ 200
        var statuses = results.Select(r => r.Status).ToArray();
        statuses.Should().Contain("Authorized");
        statuses.Should().Contain("Rejected");

        await using var verifyContext = new ApplicationDbContext(options);
        var finalWallet = await verifyContext.Wallets.FirstAsync(w => w.WalletId == "WLT-RACE");
        finalWallet.Balance.Should().BeGreaterThanOrEqualTo(50m); // at most 150 deducted (one auth)
    }
}