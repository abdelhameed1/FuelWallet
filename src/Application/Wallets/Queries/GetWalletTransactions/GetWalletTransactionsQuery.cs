using FuelWallet.Application.FuelAuthorizations.Queries.GetTransactionById;
using MediatR;

namespace FuelWallet.Application.Wallets.Queries.GetWalletTransactions;

public record GetWalletTransactionsQuery(string WalletId) : IRequest<List<TransactionDto>>;