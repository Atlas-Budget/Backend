using Api.Data;
using Api.Service.Interfaces;
using Api.Entities;
using Microsoft.EntityFrameworkCore;
using Api.Validations;
using Api.Dtos.Transactions.Get;
using Api.Dtos.Transactions.Set;
using Api.Exceptions;

namespace Api.Service.Services;

public class TransactionService : ITransactionService
{
    private readonly AppDbContext _dbContext;

    public TransactionService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }


    public async Task<Guid> CreateTransaction(Guid userId, CreateTransactionRequest request)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));

        TransactionValidations.ValidateCreateRequest(request);

        var tx = new Transaction
        {
            UserId = userId,
            Type = request.Type,
            Timing = request.Timing,
            TotalAmount = request.TotalAmount,
            Description = request.Description?.Trim(),
            StartDate = request.StartDate,
            FinishDate = request.FinishDate is null ? null : request.FinishDate,

            Frequency = request.Timing == TransactionTiming.Recurring ? request.Frequency : null,
            RecurrenceWeekDay = null,
            RecurrenceMonthDay = null,

            TotalOccurrences = request.Timing == TransactionTiming.Installment ? request.TotalOccurrences : null,

            IsReserveContribution = request.IsReserveContribution
        };

        if (tx.Timing == TransactionTiming.Recurring)
        {
            if (tx.Frequency == RecurrenceFrequency.Weekly)
                tx.RecurrenceWeekDay = request.RecurrenceWeekDay;

            if (tx.Frequency == RecurrenceFrequency.Monthly)
                tx.RecurrenceMonthDay = request.RecurrenceMonthDay;
        }

        _dbContext.Transactions.Add(tx);
        await _dbContext.SaveChangesAsync();

        return tx.Id;
    }

    public async Task<TransactionResponse> GetTransaction(Guid userId, Guid transactionId)
    {
        var tx = await _dbContext.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == transactionId && x.UserId == userId);

        if (tx is null)
            throw new NotFoundException("Transaction not found.");

        return new TransactionResponse
        {
            Id = tx.Id,
            UserId = tx.UserId,
            Type = tx.Type,
            Timing = tx.Timing,
            TotalAmount = tx.TotalAmount,
            Description = tx.Description,
            StartDate = tx.StartDate,
            FinishDate = tx.FinishDate,
            Frequency = tx.Frequency,
            RecurrenceWeekDay = tx.RecurrenceWeekDay,
            RecurrenceMonthDay = tx.RecurrenceMonthDay,
            TotalOccurrences = tx.TotalOccurrences,
            IsReserveContribution = tx.IsReserveContribution,
            CreatedAtUtc = tx.CreatedAtUtc
        };
    }


    public async Task UpdateTransaction(Guid userId, Guid transactionId, CreateTransactionRequest request)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));

        var tx = await _dbContext.Transactions
            .FirstOrDefaultAsync(x => x.Id == transactionId && x.UserId == userId);

        if (tx is null)
            throw new NotFoundException("Transaction not found.");

        TransactionValidations.ValidateCreateRequest(request);

        tx.Type = request.Type;
        tx.Timing = request.Timing;
        tx.TotalAmount = request.TotalAmount;
        tx.Description = request.Description?.Trim();
        tx.StartDate = request.StartDate;
        tx.IsReserveContribution = request.IsReserveContribution;

        tx.FinishDate = null;
        tx.Frequency = null;
        tx.RecurrenceWeekDay = null;
        tx.RecurrenceMonthDay = null;
        tx.TotalOccurrences = null;

        if (tx.Timing == TransactionTiming.Recurring)
        {
            tx.FinishDate = request.FinishDate is null ? null : request.FinishDate.Value;
            tx.Frequency = request.Frequency;

            if (tx.Frequency == RecurrenceFrequency.Weekly)
                tx.RecurrenceWeekDay = request.RecurrenceWeekDay;

            if (tx.Frequency == RecurrenceFrequency.Monthly)
                tx.RecurrenceMonthDay = request.RecurrenceMonthDay;
        }
        else if (tx.Timing == TransactionTiming.Installment)
        {
            tx.TotalOccurrences = request.TotalOccurrences;
        }

        await _dbContext.SaveChangesAsync();
    }


    public async Task<TransactionSummaryResponse> GetTransactionsResume(
    Guid userId,
    DateTime startDate,
    DateTime endDate)
    {
        if (endDate < startDate) throw new ArgumentException("endDate must be >= startDate.");

        var from = new DateTimeOffset(startDate.Date, TimeSpan.Zero);
        var to = new DateTimeOffset(endDate.Date, TimeSpan.Zero);

        var candidates = await _dbContext.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .Where(t =>
                (t.Timing == TransactionTiming.Single && t.StartDate >= from && t.StartDate <= to)
                ||
                (t.Timing == TransactionTiming.Recurring && t.StartDate <= to && (t.FinishDate == null || t.FinishDate >= from))
                ||
                (t.Timing == TransactionTiming.Installment && t.StartDate <= to)
            )
            .ToListAsync();

        decimal income = 0m, expense = 0m;
        decimal reserveIncome = 0m, reserveExpense = 0m;

        foreach (var t in candidates)
        {
            var amountInRange = TransactionAmountInRange(t, from, to);

            if (amountInRange == 0m) continue;

            if (t.Type == TransactionType.Income)
            {
                income += amountInRange;
                if (t.IsReserveContribution) reserveIncome += amountInRange;
            }
            else
            {
                expense += amountInRange;
                if (t.IsReserveContribution) reserveExpense += amountInRange;
            }
        }

        return new TransactionSummaryResponse
        {
            StartDate = startDate.Date,
            EndDate = endDate.Date,
            TotalIncome = income,
            TotalExpense = expense,
            ReserveIncome = reserveIncome,
            ReserveExpense = reserveExpense
        };
    }
    private static decimal TransactionAmountInRange(Transaction t, DateTimeOffset from, DateTimeOffset to)
    {
        return t.Timing switch
        {
            TransactionTiming.Single => SingleAmountInRange(t, from, to),
            TransactionTiming.Installment => InstallmentAmountInRange(t, from, to),
            TransactionTiming.Recurring => RecurringAmountInRange(t, from, to),
            _ => 0m
        };
    }

    private static decimal SingleAmountInRange(Transaction t, DateTimeOffset from, DateTimeOffset to)
    {
        return (t.StartDate >= from && t.StartDate <= to) ? t.TotalAmount : 0m;
    }

    private static decimal InstallmentAmountInRange(Transaction t, DateTimeOffset from, DateTimeOffset to)
    {
        if (t.TotalOccurrences is null || t.TotalOccurrences <= 0) return 0m;

        var n = t.TotalOccurrences.Value;

        var totalCents = ToCents(t.TotalAmount);
        var baseCents = totalCents / n;
        var remainder = (int)(totalCents % n);

        decimal sum = 0m;

        for (int i = 0; i < n; i++)
        {
            var due = t.StartDate.AddMonths(i);
            if (due < from || due > to) continue;

            var cents = baseCents + (i < remainder ? 1 : 0);
            sum += FromCents(cents);
        }

        return sum;
    }

    private static long ToCents(decimal amount) => (long)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);
    private static decimal FromCents(long cents) => cents / 100m;

    private static decimal RecurringAmountInRange(Transaction t, DateTimeOffset from, DateTimeOffset to)
    {
        if (t.Frequency is null) return 0m;

        var ruleStart = t.StartDate > from ? t.StartDate : from;
        var ruleEnd = t.FinishDate is not null && t.FinishDate.Value < to ? t.FinishDate.Value : to;

        if (ruleEnd < ruleStart) return 0m;

        int count = t.Frequency.Value switch
        {
            RecurrenceFrequency.Daily => CountDaily(ruleStart, ruleEnd),
            RecurrenceFrequency.Weekly => CountWeekly(ruleStart, ruleEnd, t.RecurrenceWeekDay),
            RecurrenceFrequency.Monthly => CountMonthly(ruleStart, ruleEnd, t.RecurrenceMonthDay),
            _ => 0
        };

        return count * t.TotalAmount;
    }

    private static int CountDaily(DateTimeOffset from, DateTimeOffset to)
    {
        var days = (to.Date - from.Date).Days;
        return days >= 0 ? days + 1 : 0;
    }

    private static int CountWeekly(DateTimeOffset from, DateTimeOffset to, int? weekDay)
    {
        if (weekDay is null || weekDay < 0 || weekDay > 6) return 0;

        var first = AlignToWeekday(from, weekDay.Value);
        if (first > to) return 0;

        var days = (to.Date - first.Date).Days;
        return (days / 7) + 1;
    }

    private static DateTimeOffset AlignToWeekday(DateTimeOffset d, int targetWeekDay)
    {
        var current = (int)d.DayOfWeek;
        var delta = (targetWeekDay - current + 7) % 7;
        return d.AddDays(delta);
    }

    private static int CountMonthly(DateTimeOffset from, DateTimeOffset to, int? monthDay)
    {
        if (monthDay is null || monthDay < 1 || monthDay > 31) return 0;

        var first = MonthlyOccurrenceOnOrAfter(from, monthDay.Value);
        if (first > to) return 0;

        int count = 0;
        var cur = first;

        while (cur <= to)
        {
            count++;
            cur = AddMonthsClamped(cur, 1, monthDay.Value);
        }

        return count;
    }

    private static DateTimeOffset MonthlyOccurrenceOnOrAfter(DateTimeOffset from, int monthDay)
    {
        var candidate = MakeMonthDate(from, monthDay);
        if (candidate < from.Date)
        {
            candidate = AddMonthsClamped(candidate, 1, monthDay);
        }
        return candidate;
    }

    private static DateTimeOffset MakeMonthDate(DateTimeOffset anchor, int monthDay)
    {
        var year = anchor.Year;
        var month = anchor.Month;
        var lastDay = DateTime.DaysInMonth(year, month);
        var day = Math.Min(monthDay, lastDay);

        return new DateTimeOffset(year, month, day, 0, 0, 0, anchor.Offset);
    }

    private static DateTimeOffset AddMonthsClamped(DateTimeOffset d, int months, int monthDay)
    {
        var next = d.AddMonths(months);
        return MakeMonthDate(next, monthDay);
    }

    public async Task DeleteTransaction(Guid userId, Guid transactionId)
    {
        var transaction = await _dbContext.Transactions
            .FirstOrDefaultAsync(t => t.Id == transactionId && t.UserId == userId);

        if (transaction is null)
            return;

        _dbContext.Transactions.Remove(transaction);
        await _dbContext.SaveChangesAsync();
    }

}
