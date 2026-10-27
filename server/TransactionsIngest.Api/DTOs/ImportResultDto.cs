namespace TransactionsIngest.Api.DTOs;

public class ImportResultDto
{
    public int InsertedCount { get; set; }

    public int UpdatedCount { get; set; }

    public int RevokedCount { get; set; }

    public int UnchangedCount { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime CompletedAt { get; set; }

    public string Status { get; set; } = "Completed";
}
