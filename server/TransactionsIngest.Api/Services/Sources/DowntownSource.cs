using TransactionsIngest.Api.DTOs;
using TransactionsIngest.Api.Interfaces;

namespace TransactionsIngest.Api.Services.Sources;

public class DowntownSource : ITransactionSource
{
    public string SourceSystem => "Store Downtown";

    public Task<IReadOnlyList<TransactionSourceDto>> GetTransactionsAsync(
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Fixed UTC timestamps to keep repeated snapshots identical.
        IReadOnlyList<TransactionSourceDto> transactions =
        [
            new TransactionSourceDto
            {
                SourceSystem = SourceSystem,
                TransactionId = 100001,
                CardNumber = "***0002",
                LocationCode = "DOWNTOWN",
                ProductName = "Mechanical Keyboard",
                Amount = 89.99m,
                TransactionTime = new DateTime(
                    2026, 10, 27, 9, 45, 12, DateTimeKind.Utc)
            },
            new TransactionSourceDto
            {
                SourceSystem = SourceSystem,
                TransactionId = 100003,
                CardNumber = "***3333",
                LocationCode = "DOWNTOWN",
                ProductName = "Laptop Stand",
                Amount = 39.99m,
                TransactionTime = new DateTime(
                    2026, 10, 27, 10, 0, 0, DateTimeKind.Utc)
            }
        ];

        return Task.FromResult(transactions);
    }
}
