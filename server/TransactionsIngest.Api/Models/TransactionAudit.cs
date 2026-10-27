using System.ComponentModel.DataAnnotations;

namespace TransactionsIngest.Api.Models;

public class TransactionAudit
{
    public int Id { get; set; }

    // References Transaction.Id, the internal database primary key.
    public int TransactionRecordId { get; set; }

    [Required]
    [MaxLength(30)]
    public string ChangeType { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string FieldName { get; set; } = string.Empty;

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    // Supply the event timestamp in UTC when creating the audit.
    public DateTime ChangedAt { get; set; }

    public Transaction Transaction { get; set; } = null!;
}