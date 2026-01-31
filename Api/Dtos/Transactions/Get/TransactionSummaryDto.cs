namespace Api.Dtos.Transactions.Get;

public class TransactionSummaryResponse
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }

    public decimal Net => TotalIncome - TotalExpense;

    public decimal ReserveIncome { get; set; }
    public decimal ReserveExpense { get; set; }

    public decimal ReserveNet => ReserveIncome - ReserveExpense;

}
