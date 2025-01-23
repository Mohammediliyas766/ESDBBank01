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
        private readonly IEventRepository _eventRepository;

        public BankController(IAccountRepository accountRepository, IEventStore eventStore, IEventRepository eventRepository)
        {
            _accountRepository = accountRepository;
            _eventStore = eventStore;
            _eventRepository = eventRepository;
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
            var account = await CreateAccountInternal(createAccountDto);
            await _eventRepository.SaveEventDataAsync(new EventData
            {
                EventType = nameof(AccountCreated),
                AccountNumber = account.AccountNumber,
                CustomerName = account.CustomerName,
                Balance = account.Balance
            });
            return CreatedAtAction(nameof(GetAccount), new { accountNumber = account.AccountNumber }, account);
        }

        [HttpPost("{accountNumber}/credit")]
        public async Task<IActionResult> Credit(string accountNumber, [FromBody] decimal amount)
        {
            var account = await CreditAccountInternal(accountNumber, amount);
            if (account == null)
                return NotFound();

            await _eventRepository.SaveEventDataAsync(new EventData
            {
                EventType = nameof(AmountCredited),
                AccountNumber = account.AccountNumber,
                CustomerName = account.CustomerName,
                Balance = account.Balance
            });
            return Ok(account);
        }

        [HttpPost("{accountNumber}/debit")]
        public async Task<IActionResult> Debit(string accountNumber, [FromBody] decimal amount)
        {
            var account = await DebitAccountInternal(accountNumber, amount);
            if (account == null)
                return NotFound();

            await _eventRepository.SaveEventDataAsync(new EventData
            {
                EventType = nameof(AmountDebited),
                AccountNumber = account.AccountNumber,
                CustomerName = account.CustomerName,
                Balance = account.Balance
            });
            return Ok(account);
        }

        [HttpDelete("{accountNumber}")]
        public async Task<IActionResult> DeleteAccount(string accountNumber)
        {
            var account = await DeleteAccountInternal(accountNumber);
            if (account == null)
                return NotFound();

            await _eventRepository.SaveEventDataAsync(new EventData
            {
                EventType = nameof(AccountDeleted),
                AccountNumber = account.AccountNumber,
                CustomerName = account.CustomerName,
                Balance = account.Balance
            });
            return NoContent();
        }

        private async Task<Account> CreateAccountInternal(CreateAccountDto createAccountDto)
        {
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
            await _eventRepository.SaveEventDataAsync(new EventData
            {
                EventType = @event.GetType().Name,
                AccountNumber = account.AccountNumber,
                CustomerName = account.CustomerName,
                Balance = account.Balance
            });

            return account;
        }

        private async Task<Account?> CreditAccountInternal(string accountNumber, decimal amount)
        {
            var account = await _accountRepository.GetByAccountNumberAsync(accountNumber);
            if (account == null)
                return null;

            account.Balance += amount;
            await _accountRepository.UpdateAsync(account);

            var @event = new AmountCredited(
                account.AccountNumber,
                amount,
                account.Balance
            );
            await _eventStore.SaveEventAsync($"account-{account.AccountNumber}", @event);
            await _eventRepository.SaveEventDataAsync(new EventData
            {
                EventType = @event.GetType().Name,
                AccountNumber = account.AccountNumber,
                CustomerName = account.CustomerName,
                Balance = account.Balance
            });

            return account;
        }

        private async Task<Account?> DebitAccountInternal(string accountNumber, decimal amount)
        {
            var account = await _accountRepository.GetByAccountNumberAsync(accountNumber);
            if (account == null)
                return null;

            if (account.Balance < amount)
                return null;

            account.Balance -= amount;
            await _accountRepository.UpdateAsync(account);

            var @event = new AmountDebited(
                account.AccountNumber,
                amount,
                account.Balance
            );
            await _eventStore.SaveEventAsync($"account-{account.AccountNumber}", @event);
            await _eventRepository.SaveEventDataAsync(new EventData
            {
                EventType = @event.GetType().Name,
                AccountNumber = account.AccountNumber,
                CustomerName = account.CustomerName,
                Balance = account.Balance
            });

            return account;
        }

        private async Task<Account?> DeleteAccountInternal(string accountNumber)
        {
            var account = await _accountRepository.GetByAccountNumberAsync(accountNumber);
            if (account == null)
                return null;

            await _accountRepository.DeleteAsync(accountNumber);

            var @event = new AccountDeleted(
                account.AccountNumber,
                DateTime.UtcNow
            );
            await _eventStore.SaveEventAsync($"account-{account.AccountNumber}", @event);
            await _eventRepository.SaveEventDataAsync(new EventData
            {
                EventType = @event.GetType().Name,
                AccountNumber = account.AccountNumber,
                CustomerName = account.CustomerName,
                Balance = account.Balance
            });

            return account;
        }
    }
}
