using Azure;
using BankAPI.Infrastructure.Data;
using EventStore.Client;
using System.Runtime.InteropServices;
using System.Text.Json;
using static Grpc.Core.Metadata;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        public async Task<IEnumerable<object>> GetEventsForStreamAsync(string streamName)
        {
            var events = new List<object>();
            var result = _client.ReadAllAsync(
                Direction.Forwards,
                Position.Start
            );

            await foreach (var @event in result)
            {
                if (@event.Event.EventStreamId == streamName)
                {
                    var eventData = JsonSerializer.Deserialize<object>(@event.Event.Data.Span);
                    if (eventData != null)
                        events.Add(eventData);
                }
            }

            return events;
        }
      
    }
}