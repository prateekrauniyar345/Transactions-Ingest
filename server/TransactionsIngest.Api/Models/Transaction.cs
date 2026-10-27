// server/TransactionsIngest.Api/models/Transaction.cs
using System.ComponentModel.DataAnnotations;

namespace TransactionsIngest.Api.Models;

public class Transaction
{
    public int Id { get; set; }

    [Required]
    [MaxLength(19)]
    public string SourceSystem { get; set; } = string.Empty;

    [Required]
    public int TransactionId { get; set; }

    public decimal Amount { get; set; }

    public DateTime TransactionTime { get; set; }

    // new transaction status is active by default
    public TransactionStatus Status { get; set; } = TransactionStatus.Active;
}