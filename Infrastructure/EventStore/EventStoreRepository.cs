using EventStore.Client;
using System.Text.Json;

namespace BankAPI.Infrastructure.EventStore;

public class EventStoreRepository : IEventStore
{
    private readonly EventStoreClient _client;

    public EventStoreRepository(EventStoreClient client)
    {
        _client = client;
    }

    public async Task SaveEventAsync<T>(string streamName, T eventData) where T : class
    {
        var eventType = eventData.GetType().Name;
        var data = JsonSerializer.SerializeToUtf8Bytes(eventData);
        var eventDataObj = new EventData(
            Uuid.NewUuid(),
            eventType,
            data
        );

        await _client.AppendToStreamAsync(
            streamName,
            StreamState.Any,
            new[] { eventDataObj }
        );
    }

    public async Task<IEnumerable<object>> GetEventsAsync(string streamName)
    {
        var events = new List<object>();
        var result = _client.ReadStreamAsync(
            Direction.Forwards,
            streamName,
            StreamPosition.Start
        );

        await foreach (var @event in result)
        {
            var eventData = JsonSerializer.Deserialize<object>(@event.Event.Data.Span);
            if (eventData != null)
                events.Add(eventData);
        }

        return events;
    }
}