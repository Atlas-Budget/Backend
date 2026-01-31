using Api.Dtos.Transactions.Get;
using Api.Dtos.Transactions.Set;

namespace Api.Service.Interfaces;

public interface ITransactionService
{
    Task<Guid> CreateTransaction(Guid userId, CreateTransactionRequest request);

    Task<TransactionResponse> GetTransaction(Guid userId, Guid transactionId);

    Task UpdateTransaction(Guid userId, Guid transactionId, CreateTransactionRequest request);

    Task DeleteTransaction(Guid userId, Guid transactionId);

    Task<TransactionSummaryResponse> GetTransactionsResume(
        Guid userId,
        DateTime startDate,
        DateTime endDate
    );
}
