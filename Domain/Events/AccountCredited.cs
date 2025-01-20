namespace BankAPI.Domain.Events;

public record AmountCredited(
    string AccountNumber,
    decimal Amount,
    decimal NewBalance
);