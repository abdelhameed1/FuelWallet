using FuelWallet.Application.Common.Exceptions;
using FuelWallet.Application.Common.Interfaces;
using FuelWallet.Domain.Entities;
using FuelWallet.Domain.Enums;
using FuelWallet.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FuelWallet.Application.FuelAuthorizations.Commands.CreateFuelAuthorization;

public class CreateFuelAuthorizationCommandHandler
    : IRequestHandler<CreateFuelAuthorizationCommand, FuelAuthorizationResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IOptimisticConcurrencyExecutor _concurrency;

    public CreateFuelAuthorizationCommandHandler(
        IApplicationDbContext context,
        IOptimisticConcurrencyExecutor concurrency)
    {
        _context = context;
        _concurrency = concurrency;
    }

    public async Task<FuelAuthorizationResult> Handle(
        CreateFuelAuthorizationCommand request,
        CancellationToken cancellationToken)
    {
        // Idempotency fast path: a request already processed under this reference returns its original result.
        var existing = await _context.FuelTransactions
            .FirstOrDefaultAsync(t => t.RequestReference == request.RequestReference, cancellationToken);

        if (existing != null)
            return MapToResult(existing);

        var wallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.WalletId == request.WalletId, cancellationToken);

        if (wallet == null)
            throw new NotFoundException("Wallet", request.WalletId);

        try
        {
            return await _concurrency.ExecuteAsync(
                operation: async ct => MapToResult(await ProcessAsync(wallet, request, ct)),
                onConflictExhausted: ct => RejectForConflictAsync(request, ct),
                cancellationToken);
        }
        catch (DuplicateKeyException)
        {
            // A concurrent request with the same reference won the race and inserted first
            // (the unique index is the final idempotency guard). Return that original result.
            var original = await _context.FuelTransactions
                .AsNoTracking()
                .FirstAsync(t => t.RequestReference == request.RequestReference, cancellationToken);

            return MapToResult(original);
        }
    }

    private async Task<FuelTransaction> ProcessAsync(
        Wallet wallet,
        CreateFuelAuthorizationCommand request,
        CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var alreadySpentToday = await _context.FuelTransactions
            .Where(t => t.WalletId == request.WalletId
                     && t.Status == TransactionStatus.Authorized
                     && t.CreatedAt >= today)
            .SumAsync(t => t.AuthorizedAmount ?? 0m, cancellationToken);

        var transaction = NewPendingTransaction(request);
        ApplyBusinessRules(wallet, transaction, request.RequestedAmount, alreadySpentToday);

        _context.FuelTransactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        return transaction;
    }

    private async Task<FuelAuthorizationResult> RejectForConflictAsync(
        CreateFuelAuthorizationCommand request,
        CancellationToken cancellationToken)
    {
        var transaction = NewPendingTransaction(request);
        transaction.Reject("Concurrent update conflict — please retry your request.");

        _context.FuelTransactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResult(transaction);
    }

    private static FuelTransaction NewPendingTransaction(CreateFuelAuthorizationCommand request) => new()
    {
        WalletId = request.WalletId,
        StationId = request.StationId,
        PumpId = request.PumpId,
        RequestedAmount = request.RequestedAmount,
        RequestReference = request.RequestReference,
        Status = TransactionStatus.Pending
    };

    private static void ApplyBusinessRules(
        Wallet wallet,
        FuelTransaction transaction,
        decimal requestedAmount,
        decimal alreadySpentToday)
    {
        try
        {
            wallet.EnsureActive();
            wallet.EnsureWithinDailyLimit(requestedAmount, alreadySpentToday);
            wallet.Deduct(requestedAmount);
            transaction.Authorize(requestedAmount);
        }
        catch (DomainException ex)
        {
            transaction.Reject(ex.Message);
        }
    }

    private static FuelAuthorizationResult MapToResult(FuelTransaction transaction) => new()
    {
        TransactionId = transaction.Id,
        Status = transaction.Status.ToString(),
        AuthorizedAmount = transaction.AuthorizedAmount,
        RejectionReason = transaction.RejectionReason
    };
}
