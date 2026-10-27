using TransactionsIngest.Api.DTOs;
using TransactionsIngest.Api.Interfaces;

namespace TransactionsIngest.Api.Services.Sources;

public class UptownSource : ITransactionSource
{
    public string SourceSystem => "Store Uptown";

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
                CardNumber = "***1111",
                LocationCode = "UPTOWN",
                ProductName = "Wireless Mouse",
                // Amount = 24.99m, // initial amount for this transaction
                Amount = 33.99m,  // new amount for this transaction - to test the audit history
                TransactionTime = new DateTime(
                    2026, 10, 27, 8, 15, 30, DateTimeKind.Utc)
            },

            // now we will comment this transaction to check the 
            // revoked transaction for testing purposes
            // new TransactionSourceDto
            // {
            //     SourceSystem = SourceSystem,
            //     TransactionId = 100002,
            //     CardNumber = "***2222",
            //     LocationCode = "UPTOWN",
            //     ProductName = "USB Cable",
            //     Amount = 14.99m,
            //     TransactionTime = new DateTime(
            //         2026, 10, 27, 8, 30, 0, DateTimeKind.Utc)
            // }
        ];

        return Task.FromResult(transactions);
    }
}
