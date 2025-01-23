namespace BankAPI.Domain.Models
{
    public class EventData
    {
        public int Id { get; set; }
        public required string EventType { get; set; }
        public required string AccountNumber { get; set; }
        public required string CustomerName { get; set; }
        public decimal Balance { get; set; }
    }
}