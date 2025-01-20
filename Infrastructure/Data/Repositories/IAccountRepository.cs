using BankAPI.Domain.Models;

namespace BankAPI.Infrastructure.Data.Repositories;

public interface IAccountRepository
{
    Task<IEnumerable<Account>> GetAllAsync();
    Task<Account?> GetByAccountNumberAsync(string accountNumber);
    Task<bool> AccountNumberExistsAsync(string accountNumber);
    Task CreateAsync(Account account);
    Task UpdateAsync(Account account);
    Task DeleteAsync(string accountNumber);
}