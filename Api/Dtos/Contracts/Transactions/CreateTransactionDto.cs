using Api.Entities;

namespace Api.Dtos.Contracts.Transactions;

public class CreateTransactionRequest
{
    public TransactionType Type { get; set; }
    public TransactionTiming Timing { get; set; }

    public decimal Amount { get; set; }
    public string? Description { get; set; }

    public DateTime StartDate { get; set; }

    public FrequencyType Frequency { get; set; }

    public int? RecurrenceDay { get; set; }
    public int? TotalOccurrences { get; set; }

    public bool IsReserveContribution { get; set; }
}
