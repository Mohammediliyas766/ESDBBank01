using BankAPI.Domain.Models;

namespace BankAPI.Infrastructure.EventStore
{
    public interface IEventRepository
    {
        Task SaveEventDataAsync(EventData eventData);
    }
}