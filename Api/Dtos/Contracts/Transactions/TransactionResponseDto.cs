using Api.Entities;

namespace Api.Dtos.Contracts.Transactions
{
    public class TransactionResponse
    {
        public Guid Id { get; set; }
        public TransactionType Type { get; set; } // Income, Expense, Reserve
        public TransactionTiming Timing { get; set; }       // Common, Future, Constant
        public decimal Amount { get; set; }
        public string Description { get; set; } = default!;
        public DateTime StartDate { get; set; }
        public FrequencyType Frequency { get; set; } // Daily, Weekly, Monthly, Once
        public bool IsReserveContribution { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
