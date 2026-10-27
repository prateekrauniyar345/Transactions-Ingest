using TransactionsIngest.Api.DTOs;

namespace TransactionsIngest.Api.Interfaces;

public interface ITransactionSource
{
    string SourceSystem { get; }

    // Returns a complete snapshot. A failed fetch must throw, not return an empty list.
    Task<IReadOnlyList<TransactionSourceDto>> GetTransactionsAsync(
        CancellationToken cancellationToken = default);
}
