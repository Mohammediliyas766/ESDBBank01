namespace BankAPI.Domain.Events;

public record AccountDeleted(
    string AccountNumber,
    DateTime DeletedAt
);