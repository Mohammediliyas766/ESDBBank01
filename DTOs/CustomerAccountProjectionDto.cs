namespace BankAPI.DTOs
{
    public class CustomerAccountProjectionDto
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal Balance { get; set; }
    }

    public class ProjectionResultDto
    {
        public List<CustomerAccountProjectionDto> Customers { get; set; } = new();
    }
}
