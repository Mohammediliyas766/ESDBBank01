using BankAPI.Domain.Models;
using BankAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankAPI.Infrastructure.EventStore
{
    public class EventRepository : IEventRepository
    {
        private readonly BankDbContext _dbContext;
        private readonly List<EventData> _eventBuffer = new List<EventData>();
        private const int BATCH_SIZE = 5;

        public EventRepository(BankDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SaveEventDataAsync(EventData eventData)
        {
            _eventBuffer.Add(eventData);

            if (_eventBuffer.Count >= BATCH_SIZE)
            {
                await SaveEventBatchAsync();
            }
        }

        private async Task SaveEventBatchAsync()
        {
            await _dbContext.EventData.AddRangeAsync(_eventBuffer);
            await _dbContext.SaveChangesAsync();
            _eventBuffer.Clear();
        }
    }
}