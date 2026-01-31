namespace Api.Dtos.Transactions.Get
{
    public class TransactionResponse
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public TransactionType Type { get; set; }

        public TransactionTiming Timing { get; set; }

        public decimal TotalAmount { get; set; }

        public string Description { get; set; }

        public DateTimeOffset StartDate { get; set; }

        public DateTimeOffset? FinishDate { get; set; }

        public RecurrenceFrequency? Frequency { get; set; }

        public int? RecurrenceWeekDay { get; set; }

        public int? RecurrenceMonthDay { get; set; }

        public int? TotalOccurrences { get; set; }

        public bool IsReserveContribution { get; set; }

        public DateTimeOffset CreatedAtUtc { get; set; }
    }
}
