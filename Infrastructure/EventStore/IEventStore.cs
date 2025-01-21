namespace BankAPI.Infrastructure.EventStore;

public interface IEventStore
{
    Task SaveEventAsync<T>(string streamName, T eventData) where T : class;
    Task<IEnumerable<object>> GetEventsAsync(string streamName);
}