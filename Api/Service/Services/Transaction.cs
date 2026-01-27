using Api.Data;
using Api.Dtos.Contracts.Transactions;
using Api.Service.Interfaces;
using Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Service.Services;

public class TransactionService : ITransactionService
{
    private readonly AppDbContext _dbContext;

    public TransactionService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task CreateAsync(Guid userId, CreateTransactionRequest request)
    {
        var transactions = new List<Transaction>();

        if (request.Frequency == FrequencyType.Once)
        {
            transactions.Add(BuildTransaction(userId, request, new DateTimeOffset(request.StartDate)));
        }
        else
        {
            var occurrences = request.TotalOccurrences ?? 12;
            var date = new DateTimeOffset(request.StartDate);

            for (int i = 0; i < occurrences; i++)
            {
                transactions.Add(BuildTransaction(userId, request, date));
                date = GetNextDate(date, request.Frequency);
            }
        }

        _dbContext.Transactions.AddRange(transactions);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<TransactionSummaryResponse> GetAsync(
        Guid userId,
        DateTimeOffset start,
        DateTimeOffset end)
    {
        var transactions = await _dbContext.Transactions
            .Where(t => t.UserId == userId &&
                        t.StartDate >= start &&
                        t.StartDate <= end)
            .OrderBy(t => t.StartDate)
            .ToListAsync();

        var total = transactions.Sum(t => t.Type == TransactionType.Expense ? -t.Amount : t.Amount);
        var days = (end - start).Days + 1;

        return new TransactionSummaryResponse
        {
            Transactions = transactions.Select(t => new TransactionResponse
            {
                Id = t.Id,
                Type = t.Type,
                Timing = t.Timing,
                Amount = t.Amount,
                Description = t.Description,
                StartDate = t.StartDate.UtcDateTime,
                Frequency = t.Frequency.HasValue ? (FrequencyType)t.Frequency.Value : FrequencyType.Once,
                IsReserveContribution = t.IsReserveContribution,
                CreatedAtUtc = t.CreatedAtUtc.UtcDateTime
            }).ToList(),

            Total = total,
            DailyAverage = days > 0 ? total / days : total
        };
    }

    public async Task UpdateAsync(Guid userId, Guid transactionId, CreateTransactionRequest request)
    {
        var transaction = await _dbContext.Transactions
            .FirstOrDefaultAsync(t => t.Id == transactionId && t.UserId == userId);

        if (transaction == null)
            throw new KeyNotFoundException("Transaction not found.");

        transaction.Type = request.Type;
        transaction.Timing = request.Timing;
        transaction.Amount = request.Amount;
        transaction.Description = request.Description ?? string.Empty;
        transaction.StartDate = new DateTimeOffset(request.StartDate);
        transaction.Frequency = request.Frequency == FrequencyType.Once ? null : (RecurrenceFrequency?)request.Frequency;
        transaction.RecurrenceDay = request.RecurrenceDay;
        transaction.TotalOccurrences = request.TotalOccurrences;
        transaction.IsReserveContribution = request.IsReserveContribution;

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid userId, Guid transactionId)
    {
        var transaction = await _dbContext.Transactions
            .FirstOrDefaultAsync(t => t.Id == transactionId && t.UserId == userId);

        if (transaction == null)
            throw new KeyNotFoundException("Transaction not found.");

        _dbContext.Transactions.Remove(transaction);
        await _dbContext.SaveChangesAsync();
    }

    private static Transaction BuildTransaction(Guid userId, CreateTransactionRequest request, DateTimeOffset date)
    {
        return new Transaction
        {
            UserId = userId,
            Type = request.Type,
            Timing = request.Timing,
            Amount = request.Amount,
            Description = request.Description ?? string.Empty,
            StartDate = date,
            Frequency = request.Frequency == FrequencyType.Once ? null : (RecurrenceFrequency?)request.Frequency,
            RecurrenceDay = request.RecurrenceDay,
            TotalOccurrences = request.TotalOccurrences,
            IsReserveContribution = request.IsReserveContribution,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
    }

    private static DateTimeOffset GetNextDate(DateTimeOffset current, FrequencyType frequency)
    {
        return frequency switch
        {
            FrequencyType.Daily => current.AddDays(1),
            FrequencyType.Weekly => current.AddDays(7),
            FrequencyType.Monthly => current.AddMonths(1),
            _ => current
        };
    }
}
