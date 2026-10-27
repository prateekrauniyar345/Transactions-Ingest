using TransactionsIngest.Api.DTOs;
using TransactionsIngest.Api.Interfaces;

namespace TransactionsIngest.Tests.Fakes;

public class FakeTransactionSource : ITransactionSource
{
    public string SourceSystem { get; }

    public IReadOnlyList<TransactionSourceDto> Transactions { get; set; }
        = [];

    public bool ShouldFail { get; set; }

    public FakeTransactionSource(string sourceSystem)
    {
        SourceSystem = sourceSystem;
    }

    public Task<IReadOnlyList<TransactionSourceDto>> GetTransactionsAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (ShouldFail)
        {
            throw new InvalidOperationException(
                "Simulated source system failure");
        }

        return Task.FromResult(Transactions);
    }
}
