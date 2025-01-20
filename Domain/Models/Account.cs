namespace BankAPI.Domain.Models;

public class Account
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public decimal Balance { get; set; }
}