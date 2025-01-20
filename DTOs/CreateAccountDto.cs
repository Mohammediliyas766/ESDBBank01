namespace BankAPI.DTOs;

public class CreateAccountDto
{
    public string CustomerName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
}