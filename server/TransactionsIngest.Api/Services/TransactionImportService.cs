using System.Globalization;
using Microsoft.EntityFrameworkCore;
using TransactionsIngest.Api.Data;
using TransactionsIngest.Api.DTOs;
using TransactionsIngest.Api.Interfaces;
using TransactionsIngest.Api.Models;

namespace TransactionsIngest.Api.Services;

public class TransactionImportService : ITransactionImportService
{
    private readonly AppDbContext _dbContext;
    private readonly IEnumerable<ITransactionSource> _sources;
    private readonly ILogger<TransactionImportService> _logger;

    public TransactionImportService(
        AppDbContext dbContext,
        IEnumerable<ITransactionSource> sources,
        ILogger<TransactionImportService> logger)
    {
        _dbContext = dbContext;
        _sources = sources;
        _logger = logger;
    }

    public async Task<ImportResultDto> ImportAsync(
        CancellationToken cancellationToken = default
    )
    {
        var result = new ImportResultDto
        {
            StartedAt = DateTime.UtcNow
        };

        // Fetch snapshots before changing the database.
        // A failed fetch must not be treated as an empty snapshot.
        var snapshots = new List<(
            string SourceSystem,
            IReadOnlyList<TransactionSourceDto> Transactions)>();

        foreach (var source in _sources)
        {
            var transactions = await source.GetTransactionsAsync(cancellationToken);
            snapshots.Add((source.SourceSystem, transactions));
        }

        var now = DateTime.UtcNow;

        // Keep database updates and audit records together.
        await using var dbTransaction =
            await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        foreach (var snapshot in snapshots)
        {
            var existingTransactions = await _dbContext.Transactions
                .Where(t => t.SourceSystem == snapshot.SourceSystem)
                .ToListAsync(cancellationToken);

            var existingById = existingTransactions
                .ToDictionary(t => t.TransactionId);

            var incomingIds = new HashSet<int>();

            foreach (var incoming in snapshot.Transactions)
            {
                if (incoming.SourceSystem != snapshot.SourceSystem)
                {
                    throw new InvalidOperationException("Source system mismatch.");
                }

                if (!incomingIds.Add(incoming.TransactionId))
                {
                    throw new InvalidOperationException($"Duplicate transaction ID: {incoming.TransactionId}");
                }

                if (!existingById.TryGetValue(incoming.TransactionId, out var existing))
                {
                    var transaction = new Transaction
                    {
                        SourceSystem = incoming.SourceSystem,
                        TransactionId = incoming.TransactionId,
                        CardNumber = incoming.CardNumber,
                        LocationCode = incoming.LocationCode,
                        ProductName = incoming.ProductName,
                        Amount = incoming.Amount,
                        TransactionTime = incoming.TransactionTime,
                        Status = TransactionStatus.Active
                    };

                    _dbContext.Transactions.Add(transaction);

                    result.InsertedCount++;
                    continue;
                }

                bool changed = false;

                void UpdateField<T>(
                    string fieldName,
                    T oldValue,
                    T newValue,
                    Action<T> applyChange)
                {
                    if (EqualityComparer<T>.Default.Equals(oldValue, newValue))
                    {
                        return;
                    }

                    applyChange(newValue);

                    _dbContext.TransactionAudits.Add(
                        new TransactionAudit
                        {
                            TransactionRecordId = existing.Id,
                            ChangeType = "Updated",
                            FieldName = fieldName,
                            OldValue = FormatAuditValue(oldValue),
                            NewValue = FormatAuditValue(newValue),
                            ChangedAt = now
                        });

                    changed = true;
                }

                UpdateField(
                    nameof(Transaction.CardNumber),
                    existing.CardNumber,
                    incoming.CardNumber,
                    value => existing.CardNumber = value);

                UpdateField(
                    nameof(Transaction.LocationCode),
                    existing.LocationCode,
                    incoming.LocationCode,
                    value => existing.LocationCode = value);

                UpdateField(
                    nameof(Transaction.ProductName),
                    existing.ProductName,
                    incoming.ProductName,
                    value => existing.ProductName = value);

                UpdateField(
                    nameof(Transaction.Amount),
                    existing.Amount,
                    incoming.Amount,
                    value => existing.Amount = value);

                UpdateField(
                    nameof(Transaction.TransactionTime),
                    existing.TransactionTime,
                    incoming.TransactionTime,
                    value => existing.TransactionTime = value);

                // If previously revoked transaction has reappeared - then reactivate it.
                if (existing.Status == TransactionStatus.Revoked)
                {
                    UpdateField(
                        nameof(Transaction.Status),
                        existing.Status,
                        TransactionStatus.Active,
                        value => existing.Status = value);
                }

                if (changed)
                {
                    result.UpdatedCount++;
                }
                else
                {
                    result.UnchangedCount++;
                }
            }

            // Check transactions missing from this source snapshot.
            foreach (var existing in existingTransactions)
            {
                if (incomingIds.Contains(existing.TransactionId))
                {
                    continue;
                }

                if (existing.Status == TransactionStatus.Revoked)
                {
                    continue;
                }

                // Revoke missing transactions
                // within 24 hours of their transaction time.
                var age = now - existing.TransactionTime;

                if (age < TimeSpan.Zero ||
                    age > TimeSpan.FromHours(24))
                {
                    continue;
                }

                var previousStatus = existing.Status;

                existing.Status = TransactionStatus.Revoked;

                _dbContext.TransactionAudits.Add(
                    new TransactionAudit
                    {
                        TransactionRecordId = existing.Id,
                        ChangeType = "Revoked",
                        FieldName = nameof(Transaction.Status),
                        OldValue = previousStatus.ToString(),
                        NewValue = TransactionStatus.Revoked.ToString(),
                        ChangedAt = now
                    });

                result.RevokedCount++;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await dbTransaction.CommitAsync(cancellationToken);

        result.CompletedAt = DateTime.UtcNow;
        result.Status = "Completed";

        _logger.LogInformation(
            "Import completed. Inserted: {Inserted}, Updated: {Updated}, Revoked: {Revoked}, Unchanged: {Unchanged}",
            result.InsertedCount,
            result.UpdatedCount,
            result.RevokedCount,
            result.UnchangedCount);

        return result;
    }

    private static string? FormatAuditValue<T>(T value) => value switch
    {
        null => null,
        DateTime timestamp => DateTime.SpecifyKind(timestamp, DateTimeKind.Utc)
            .ToString("O", CultureInfo.InvariantCulture),
        decimal amount => amount.ToString("0.00", CultureInfo.InvariantCulture),
        IFormattable formatted => formatted.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString()
    };

}
