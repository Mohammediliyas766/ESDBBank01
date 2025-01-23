using BankAPI.Domain.Events;
using BankAPI.Infrastructure.Data;
using BankAPI.Infrastructure.EventStore;
using EventStore.Client;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BankAPI.Infrastructure.EventStore
{
    public class EventStoreRepository : IEventStore
    {
        private readonly EventStoreClient _client;
        private readonly BankDbContext _dbContext;

        public EventStoreRepository(EventStoreClient client, BankDbContext dbContext)
        {
            _client = client;
            _dbContext = dbContext;
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

        public async Task<IEnumerable<object>> GetEventsAsync(string accountNumberPrefix)
        {
            var events = new List<object>();
            var result = _client.ReadAllAsync(
                Direction.Forwards,
                Position.Start
            );

            await foreach (var @event in result)
            {
                var streamName = @event.Event.EventStreamId; // Updated property
                if (streamName.StartsWith(accountNumberPrefix))
                {
                    var eventData = JsonSerializer.Deserialize<object>(@event.Event.Data.Span);
                    if (eventData != null)
                        events.Add(eventData);
                }
            }

            return events;
        }


        private string GetAccountNumberFromStreamName(string streamName)
        {
            var parts = streamName.Split('-');
            if (parts.Length > 1)
                return parts[1];
            return string.Empty;
        }
    }
}