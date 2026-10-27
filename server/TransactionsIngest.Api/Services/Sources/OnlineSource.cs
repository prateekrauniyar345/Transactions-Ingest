using TransactionsIngest.Api.DTOs;
using TransactionsIngest.Api.Interfaces;

namespace TransactionsIngest.Api.Services.Sources;

public class OnlineSource : ITransactionSource
{
    public string SourceSystem => "Online";

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
                CardNumber = "***4444",
                LocationCode = "ONLINE",
                ProductName = "USB-C Dock",
                Amount = 129.95m,
                TransactionTime = new DateTime(
                    2026, 10, 27, 11, 20, 5, DateTimeKind.Utc)
            },
            new TransactionSourceDto
            {
                SourceSystem = SourceSystem,
                TransactionId = 100004,
                CardNumber = "",
                LocationCode = "ONLINE",
                ProductName = "Webcam",
                Amount = 59.99m,
                TransactionTime = new DateTime(
                    2026, 10, 27, 11, 30, 0, DateTimeKind.Utc)
            }
        ];

        return Task.FromResult(transactions);
    }
}
