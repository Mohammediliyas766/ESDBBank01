using BankAPI.Domain.Events;
using BankAPI.Domain.Models;
using BankAPI.DTOs;
using BankAPI.Infrastructure.Data.Repositories;
using BankAPI.Infrastructure.EventStore;
using Microsoft.AspNetCore.Mvc;

namespace BankAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BankController : ControllerBase
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IEventStore _eventStore;

        public BankController(IAccountRepository accountRepository, IEventStore eventStore)
        {
            _accountRepository = accountRepository;
            _eventStore = eventStore;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Account>>> GetAllAccounts()
        {
            var accounts = await _accountRepository.GetAllAsync();
            return Ok(accounts);
        }

        [HttpGet("{accountNumber}")]
        public async Task<ActionResult<Account>> GetAccount(string accountNumber)
        {
            var account = await _accountRepository.GetByAccountNumberAsync(accountNumber);
            if (account == null)
                return NotFound();

            return Ok(account);
        }

        [HttpPost]
        public async Task<ActionResult<Account>> CreateAccount([FromBody] CreateAccountDto createAccountDto)
        {
            if (await _accountRepository.AccountNumberExistsAsync(createAccountDto.AccountNumber))
                return BadRequest("Account number already exists");

            var account = new Account
            {
                Id = Guid.NewGuid(),
                CustomerName = createAccountDto.CustomerName,
                AccountNumber = createAccountDto.AccountNumber,
                Balance = 0
            };

            await _accountRepository.CreateAsync(account);

            var @event = new AccountCreated(
                account.Id,
                account.CustomerName,
                account.AccountNumber,
                account.Balance
            );
            await _eventStore.SaveEventAsync($"account-{account.AccountNumber}", @event);

            return CreatedAtAction(nameof(GetAccount), new { accountNumber = account.AccountNumber }, account);
        }

        [HttpPost("{accountNumber}/credit")]
        public async Task<IActionResult> Credit(string accountNumber, [FromBody] decimal amount)
        {
            var account = await _accountRepository.GetByAccountNumberAsync(accountNumber);
            if (account == null)
                return NotFound();

            if (amount <= 0)
                return BadRequest("Amount must be greater than 0");

            account.Balance += amount;
            await _accountRepository.UpdateAsync(account);

            var @event = new AmountCredited(account.AccountNumber, amount, account.Balance);
            await _eventStore.SaveEventAsync($"account-{account.AccountNumber}", @event);

            return Ok(account);
        }

        [HttpPost("{accountNumber}/debit")]
        public async Task<IActionResult> Debit(string accountNumber, [FromBody] decimal amount)
        {
            var account = await _accountRepository.GetByAccountNumberAsync(accountNumber);
            if (account == null)
                return NotFound();

            if (amount <= 0)
                return BadRequest("Amount must be greater than 0");

            if (account.Balance < amount)
                return BadRequest("Insufficient balance");

            account.Balance -= amount;
            await _accountRepository.UpdateAsync(account);

            var @event = new AmountDebited(account.AccountNumber, amount, account.Balance);
            await _eventStore.SaveEventAsync($"account-{account.AccountNumber}", @event);

            return Ok(account);
        }

        [HttpDelete("{accountNumber}")]
        public async Task<IActionResult> DeleteAccount(string accountNumber)
        {
            var account = await _accountRepository.GetByAccountNumberAsync(accountNumber);
            if (account == null)
                return NotFound();

            if (account.Balance > 0)
                return BadRequest("Cannot delete account with positive balance");

            await _accountRepository.DeleteAsync(accountNumber);

            var @event = new AccountDeleted(accountNumber, DateTime.UtcNow);
            await _eventStore.SaveEventAsync($"account-{accountNumber}", @event);

            return NoContent();
        }

    }
}