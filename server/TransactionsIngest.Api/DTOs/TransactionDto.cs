namespace TransactionsIngest.Api.DTOs;

public class TransactionDto
{
    public int Id { get; set; }
    public string SourceSystem { get; set; } = string.Empty;
    public int TransactionId { get; set; }
    public string CardNumber { get; set; } = string.Empty;
    public string LocationCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime TransactionTime { get; set; }
    public string Status { get; set; } = string.Empty;
}
