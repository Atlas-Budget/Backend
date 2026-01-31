namespace Api.Entities;

public class Transaction
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public TransactionType Type { get; set; } // Income or Expense

    public TransactionTiming Timing { get; set; } //Single, Recurring, or Installment

    public decimal TotalAmount { get; set; } // allways the full amount, even for installments

    public string Description { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? FinishDate { get; set; } // if recurring and you finish it

    public RecurrenceFrequency? Frequency { get; set; } // Daily, Weekly, Monthly

    public int? RecurrenceWeekDay { get; set; } //if weekly, 0=Sunday, 6=Saturday

    public int? RecurrenceMonthDay { get; set; } //if monthly, 1-28/29/30/31 depending on month

    public int? TotalOccurrences { get; set; } //for installments

    public bool IsReserveContribution { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
