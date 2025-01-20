namespace BankAPI.Domain.Events;

public record AmountDebited(
    string AccountNumber,
    decimal Amount,
    decimal NewBalance
);