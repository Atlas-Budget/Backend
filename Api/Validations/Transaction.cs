using Api.Dtos.Transactions.Set;

namespace Api.Validations
{
    public class TransactionValidations
    {
        public static void ValidateCreateRequest(CreateTransactionRequest request)
        {
            if (request.TotalAmount <= 0)
                throw new ArgumentException("TotalAmount must be greater than zero.");

            if (string.IsNullOrWhiteSpace(request.Description))
                throw new ArgumentException("Description is required.");

            // FinishDate (quando informado) não pode ser menor que StartDate
            if (request.FinishDate is not null && request.FinishDate.Value < request.StartDate)
                throw new ArgumentException("FinishDate must be >= StartDate.");

            switch (request.Timing)
            {
                case TransactionTiming.Single:
                    if (request.Frequency is not null)
                        throw new ArgumentException("Frequency must be null for Single transactions.");
                    if (request.TotalOccurrences is not null)
                        throw new ArgumentException("TotalOccurrences must be null for Single transactions.");
                    if (request.RecurrenceWeekDay is not null || request.RecurrenceMonthDay is not null)
                        throw new ArgumentException("Recurrence fields must be null for Single transactions.");
                    if (request.FinishDate is not null)
                        throw new ArgumentException("FinishDate must be null for Single transactions.");
                    break;

                case TransactionTiming.Recurring:
                    if (request.Frequency is null)
                        throw new ArgumentException("Frequency is required for Recurring transactions.");
                    if (request.TotalOccurrences is not null)
                        throw new ArgumentException("TotalOccurrences must be null for Recurring transactions.");

                    if (request.Frequency == RecurrenceFrequency.Weekly)
                    {
                        if (request.RecurrenceWeekDay is null || request.RecurrenceWeekDay is < 0 or > 6)
                            throw new ArgumentException("RecurrenceWeekDay must be between 0 and 6 for Weekly recurrence.");
                        if (request.RecurrenceMonthDay is not null)
                            throw new ArgumentException("RecurrenceMonthDay must be null for Weekly recurrence.");
                    }

                    if (request.Frequency == RecurrenceFrequency.Monthly)
                    {
                        if (request.RecurrenceMonthDay is null || request.RecurrenceMonthDay is < 1 or > 31)
                            throw new ArgumentException("RecurrenceMonthDay must be between 1 and 31 for Monthly recurrence.");
                        if (request.RecurrenceWeekDay is not null)
                            throw new ArgumentException("RecurrenceWeekDay must be null for Monthly recurrence.");
                    }

                    if (request.Frequency == RecurrenceFrequency.Daily)
                    {
                        if (request.RecurrenceWeekDay is not null || request.RecurrenceMonthDay is not null)
                            throw new ArgumentException("Recurrence day fields must be null for Daily recurrence.");
                    }
                    break;

                case TransactionTiming.Installment:
                    if (request.TotalOccurrences is null || request.TotalOccurrences < 2 || request.TotalOccurrences > 360)
                        throw new ArgumentException("TotalOccurrences must be between 2 and 360 for Installment transactions.");
                    if (request.Frequency is not null)
                        throw new ArgumentException("Frequency must be null for Installment transactions.");
                    if (request.FinishDate is not null)
                        throw new ArgumentException("FinishDate must be null for Installment transactions.");
                    if (request.RecurrenceWeekDay is not null || request.RecurrenceMonthDay is not null)
                        throw new ArgumentException("Recurrence fields must be null for Installment transactions.");
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(request.Timing), "Invalid Timing value.");
            }
        }

    }
}
