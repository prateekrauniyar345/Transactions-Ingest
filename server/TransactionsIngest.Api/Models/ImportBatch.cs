namespace TransactionsIngest.Api.Models;

public class ImportBatch
{
    public int Id { get; set; }

    // Set explicitly to UTC when starting an import.
    public DateTime StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string Status { get; set; } = "Running";

    public int InsertedCount { get; set; }

    public int UpdatedCount { get; set; }

    public int RevokedCount { get; set; }
}