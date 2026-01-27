namespace Api.Entities;

public class Transaction
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public TransactionType Type { get; set; }

    public TransactionTiming Timing { get; set; }

    public decimal Amount { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTimeOffset StartDate { get; set; }

    public RecurrenceFrequency? Frequency { get; set; }

    public int? RecurrenceDay { get; set; }

    public int? TotalOccurrences { get; set; }

    public bool IsReserveContribution { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
