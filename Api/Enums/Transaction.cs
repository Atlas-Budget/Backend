public enum TransactionType
{
    Income = 1,
    Expense = 2
}
public enum TransactionTiming
{
    Single = 1,        // Uma vez só
    Recurring = 2,     // Para sempre (ou até parar)
    Installment = 3    
}
public enum RecurrenceFrequency
{
    Daily = 1,
    Weekly = 2,
    Monthly = 3
}
public enum FrequencyType
{
    Once = 0,
    Daily = 1,
    Weekly = 2,
    Monthly = 3
}