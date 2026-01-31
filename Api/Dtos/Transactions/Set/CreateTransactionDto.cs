using Api.Entities;

namespace Api.Dtos.Transactions.Set;

public class CreateTransactionRequest
{
    public TransactionType Type { get; set; }

    public TransactionTiming Timing { get; set; }

    public decimal TotalAmount { get; set; }

    public string Description { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? FinishDate { get; set; }

    public RecurrenceFrequency? Frequency { get; set; }

    public int? RecurrenceWeekDay { get; set; }

    public int? RecurrenceMonthDay { get; set; }

    public int? TotalOccurrences { get; set; }

    public bool IsReserveContribution { get; set; }
}
