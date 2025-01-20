using BankAPI.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankAPI.Infrastructure.Data.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly BankDbContext _context;

    public AccountRepository(BankDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Account>> GetAllAsync()
    {
        return await _context.Accounts.ToListAsync();
    }

    public async Task<Account?> GetByAccountNumberAsync(string accountNumber)
    {
        return await _context.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);
    }

    public async Task<bool> AccountNumberExistsAsync(string accountNumber)
    {
        return await _context.Accounts.AnyAsync(a => a.AccountNumber == accountNumber);
    }

    public async Task CreateAsync(Account account)
    {
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Account account)
    {
        _context.Accounts.Update(account);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string accountNumber)
    {
        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);
        if (account != null)
        {
            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
        }
    }
}