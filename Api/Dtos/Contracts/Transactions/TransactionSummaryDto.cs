namespace Api.Dtos.Contracts.Transactions;

public class TransactionSummaryResponse
{
    public List<TransactionResponse> Transactions { get; set; } = new();

    public decimal Total { get; set; }
    public decimal DailyAverage { get; set; }
}