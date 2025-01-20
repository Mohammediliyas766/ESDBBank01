namespace BankAPI.Domain.Events;

public record AccountCreated(
    Guid Id,
    string CustomerName,
    string AccountNumber,
    decimal InitialBalance
);