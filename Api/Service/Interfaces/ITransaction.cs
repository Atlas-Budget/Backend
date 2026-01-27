using Api.Dtos.Contracts.Transactions;

namespace Api.Service.Interfaces;

public interface ITransactionService
{
	Task CreateAsync(Guid userId, CreateTransactionRequest request);

	Task<TransactionSummaryResponse> GetAsync(
			Guid userId,
			DateTimeOffset start,
			DateTimeOffset end);

	Task UpdateAsync(
		Guid userId,
		Guid transactionId,
		CreateTransactionRequest request
	);

	Task DeleteAsync(Guid userId, Guid transactionId);
}
