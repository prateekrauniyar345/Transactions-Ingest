using TransactionsIngest.Api.DTOs;

namespace TransactionsIngest.Api.Interfaces;

public interface ITransactionImportService
{
    Task<ImportResultDto> ImportAsync(
        CancellationToken cancellationToken = default);
}
