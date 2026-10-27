using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using TransactionsIngest.Api.Data;
using TransactionsIngest.Api.DTOs;
using TransactionsIngest.Api.Interfaces;
using TransactionsIngest.Api.Models;
using TransactionsIngest.Api.Services;
using TransactionsIngest.Tests.Fakes;
using Xunit;

namespace TransactionsIngest.Tests;

public class TransactionImportServiceTests
{
    private static TransactionSourceDto CreateTransaction(
        string source = "Store Uptown",
        int id = 100001,
        decimal amount = 24.99m,
        DateTime? transactionTime = null)
    {
        return new TransactionSourceDto
        {
            SourceSystem = source,
            TransactionId = id,
            CardNumber = "***1111",
            LocationCode = "UPTOWN",
            ProductName = "Wireless Mouse",
            Amount = amount,
            TransactionTime = transactionTime
                ?? DateTime.UtcNow.AddHours(-1)
        };
    }

    private static async Task<TestEnvironment> CreateEnvironmentAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var failure = new FailAfterSaveInterceptor();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .AddInterceptors(failure)
            .Options;

        var db = new AppDbContext(options);
        await db.Database.MigrateAsync();

        var source = new FakeTransactionSource("Store Uptown");

        var service = new TransactionImportService(
            db,
            new ITransactionSource[] { source },
            NullLogger<TransactionImportService>.Instance);

        return new TestEnvironment(connection, db, source, service, failure);
    }

    private sealed class TestEnvironment : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        public AppDbContext Db { get; }
        public FakeTransactionSource Source { get; }
        public TransactionImportService Service { get; }
        public FailAfterSaveInterceptor Failure { get; }

        public TestEnvironment(
            SqliteConnection connection,
            AppDbContext db,
            FakeTransactionSource source,
            TransactionImportService service,
            FailAfterSaveInterceptor failure)
        {
            _connection = connection;
            Db = db;
            Source = source;
            Service = service;
            Failure = failure;
        }

        public async ValueTask DisposeAsync()
        {
            await Db.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task Import_NewTransaction_InsertsIntoDatabase()
    {
        await using var env = await CreateEnvironmentAsync();

        env.Source.Transactions =
        [
            CreateTransaction()
        ];

        var result = await env.Service.ImportAsync();

        Assert.Equal(1, result.InsertedCount);
        Assert.Equal(0, result.UpdatedCount);

        var transaction = await env.Db.Transactions.SingleAsync();

        Assert.Equal("Store Uptown", transaction.SourceSystem);
        Assert.Equal(100001, transaction.TransactionId);
        Assert.Equal(24.99m, transaction.Amount);
        Assert.Equal(TransactionStatus.Active, transaction.Status);
    }

    [Fact]
    public async Task Import_IdenticalSnapshot_DoesNotDuplicateTransactions()
    {
        await using var env = await CreateEnvironmentAsync();

        env.Source.Transactions = [CreateTransaction()];

        await env.Service.ImportAsync();
        var second = await env.Service.ImportAsync();

        Assert.Equal(0, second.InsertedCount);
        Assert.Equal(0, second.UpdatedCount);
        Assert.Equal(1, second.UnchangedCount);

        Assert.Equal(1, await env.Db.Transactions.CountAsync());
        Assert.Equal(0, await env.Db.TransactionAudits.CountAsync());
    }

    [Fact]
    public async Task Import_ChangedAmount_UpdatesAndCreatesAudit()
    {
        await using var env = await CreateEnvironmentAsync();

        var original = CreateTransaction(amount: 24.99m);

        env.Source.Transactions = [original];
        await env.Service.ImportAsync();

        env.Source.Transactions =
        [
            CreateTransaction(
                amount: 33.99m,
                transactionTime: original.TransactionTime)
        ];

        var result = await env.Service.ImportAsync();

        Assert.Equal(1, result.UpdatedCount);

        var transaction = await env.Db.Transactions.SingleAsync();
        Assert.Equal(33.99m, transaction.Amount);

        var audit = await env.Db.TransactionAudits.SingleAsync();

        Assert.Equal("Updated", audit.ChangeType);
        Assert.Equal("Amount", audit.FieldName);
        Assert.Equal("24.99", audit.OldValue);
        Assert.Equal("33.99", audit.NewValue);
        Assert.Equal(transaction.Id, audit.TransactionRecordId);
    }

    [Fact]
    public async Task Import_MissingRecentTransaction_RevokesAndAudits()
    {
        await using var env = await CreateEnvironmentAsync();

        env.Source.Transactions = [CreateTransaction()];
        await env.Service.ImportAsync();

        env.Source.Transactions = [];

        var result = await env.Service.ImportAsync();

        Assert.Equal(1, result.RevokedCount);

        var transaction = await env.Db.Transactions.SingleAsync();
        Assert.Equal(TransactionStatus.Revoked, transaction.Status);

        var audit = await env.Db.TransactionAudits.SingleAsync();

        Assert.Equal("Revoked", audit.ChangeType);
        Assert.Equal("Status", audit.FieldName);
        Assert.Equal("Active", audit.OldValue);
        Assert.Equal("Revoked", audit.NewValue);
    }

    [Fact]
    public async Task Import_AlreadyRevokedTransaction_DoesNotRevokeAgain()
    {
        await using var env = await CreateEnvironmentAsync();

        env.Source.Transactions = [CreateTransaction()];
        await env.Service.ImportAsync();

        env.Source.Transactions = [];

        await env.Service.ImportAsync();
        var third = await env.Service.ImportAsync();

        Assert.Equal(0, third.RevokedCount);
        Assert.Equal(1, await env.Db.TransactionAudits.CountAsync());
    }

    [Fact]
    public async Task Import_RevokedTransactionReappears_ReactivatesIt()
    {
        await using var env = await CreateEnvironmentAsync();

        var transaction = CreateTransaction();

        env.Source.Transactions = [transaction];
        await env.Service.ImportAsync();

        env.Source.Transactions = [];
        await env.Service.ImportAsync();

        env.Source.Transactions = [transaction];

        var result = await env.Service.ImportAsync();

        Assert.Equal(1, result.UpdatedCount);

        var stored = await env.Db.Transactions.SingleAsync();
        Assert.Equal(TransactionStatus.Active, stored.Status);

        Assert.Equal(
            2,
            await env.Db.TransactionAudits.CountAsync());
    }

    [Fact]
    public async Task Import_OlderThan24Hours_DoesNotRevoke()
    {
        await using var env = await CreateEnvironmentAsync();

        env.Source.Transactions =
        [
            CreateTransaction(
                transactionTime: DateTime.UtcNow.AddHours(-25))
        ];

        await env.Service.ImportAsync();

        env.Source.Transactions = [];

        var result = await env.Service.ImportAsync();

        Assert.Equal(0, result.RevokedCount);

        var transaction = await env.Db.Transactions.SingleAsync();
        Assert.Equal(TransactionStatus.Active, transaction.Status);
    }

    [Fact]
    public async Task Import_SourceFailure_RecordsFailedBatchWithoutChanges()
    {
        await using var env = await CreateEnvironmentAsync();

        env.Source.ShouldFail = true;

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => env.Service.ImportAsync());

        Assert.Equal(0, await env.Db.Transactions.CountAsync());
        Assert.Equal(0, await env.Db.TransactionAudits.CountAsync());

        var batch = await env.Db.ImportBatches.SingleAsync();

        Assert.Equal("Failed", batch.Status);
        Assert.NotNull(batch.CompletedAt);
        Assert.Equal(0, batch.InsertedCount);
    }

    [Fact]
    public async Task Import_DuplicateIdsWithinSource_RollsBack()
    {
        await using var env = await CreateEnvironmentAsync();

        env.Source.Transactions =
        [
            CreateTransaction(id: 100001),
            CreateTransaction(id: 100001)
        ];

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => env.Service.ImportAsync());

        Assert.Equal(0, await env.Db.Transactions.CountAsync());
        Assert.Equal(0, await env.Db.TransactionAudits.CountAsync());

        var batch = await env.Db.ImportBatches.SingleAsync();
        Assert.Equal("Failed", batch.Status);
    }

    [Fact]
    public async Task Import_SuccessfulExecution_SavesBatchCounts()
    {
        await using var env = await CreateEnvironmentAsync();

        env.Source.Transactions =
        [
            CreateTransaction(id: 100001),
            CreateTransaction(id: 100002)
        ];

        var result = await env.Service.ImportAsync();

        var batch = await env.Db.ImportBatches.SingleAsync();

        Assert.Equal("Completed", batch.Status);
        Assert.NotNull(batch.CompletedAt);
        Assert.Equal(2, batch.InsertedCount);
        Assert.Equal(result.InsertedCount, batch.InsertedCount);
        Assert.Equal(result.UnchangedCount, batch.UnchangedCount);
    }
[Fact]
public async Task Import_SameIdDifferentSources_InsertsBoth()
{
    await using var env = await CreateEnvironmentAsync();

    var downtown = new FakeTransactionSource("Store Downtown")
    {
        Transactions =
        [
            CreateTransaction(
                source: "Store Downtown",
                id: 100001)
        ]
    };

    env.Source.Transactions =
    [
        CreateTransaction(
            source: "Store Uptown",
            id: 100001)
    ];

    var service = new TransactionImportService(
        env.Db,
        new ITransactionSource[] { env.Source, downtown },
        NullLogger<TransactionImportService>.Instance);

    var result = await service.ImportAsync();

    Assert.Equal(2, result.InsertedCount);
    Assert.Equal(2, await env.Db.Transactions.CountAsync());

    var sources = await env.Db.Transactions
        .Select(t => t.SourceSystem)
        .ToListAsync();

    Assert.Contains("Store Uptown", sources);
    Assert.Contains("Store Downtown", sources);
}

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class FailAfterSaveInterceptor : SaveChangesInterceptor
    {
        public bool Armed { get; set; }

        public override ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            if (Armed && eventData.Context!.ChangeTracker.Entries<TransactionAudit>().Any())
            {
                Armed = false;
                throw new InvalidOperationException("Simulated failure after SQL writes");
            }

            return ValueTask.FromResult(result);
        }
    }

    [Fact]
    public async Task Import_FailureAfterDatabaseWrites_RollsBackUpdateAuditAndBatchCounts()
    {
        await using var env = await CreateEnvironmentAsync();
        var original = CreateTransaction();
        env.Source.Transactions = [original];
        await env.Service.ImportAsync();
        env.Source.Transactions = [CreateTransaction(
            amount: 33.99m, transactionTime: original.TransactionTime)];
        env.Failure.Armed = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => env.Service.ImportAsync());

        // Read persisted data after discarding tracked instances.
        env.Db.ChangeTracker.Clear();
        Assert.Equal(24.99m, (await env.Db.Transactions.SingleAsync()).Amount);
        Assert.Empty(await env.Db.TransactionAudits.ToListAsync());
        var batch = await env.Db.ImportBatches.OrderByDescending(b => b.Id).FirstAsync();
        Assert.Equal("Failed", batch.Status);
        Assert.NotNull(batch.CompletedAt);
        Assert.Equal(0, batch.InsertedCount);
        Assert.Equal(0, batch.UpdatedCount);
        Assert.Equal(0, batch.RevokedCount);
        Assert.Equal(0, batch.UnchangedCount);

        // The same context can perform a successful retry after failure cleanup.
        var retry = await env.Service.ImportAsync();
        Assert.Equal(1, retry.UpdatedCount);
        Assert.Single(await env.Db.TransactionAudits.ToListAsync());
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(86399, true)]
    [InlineData(86400, true)]
    [InlineData(86401, false)]
    [InlineData(-1, false)]
    public async Task Import_MissingTransaction_RespectsExact24HourWindow(
        int ageSeconds, bool revoked)
    {
        await using var env = await CreateEnvironmentAsync();
        var now = new DateTimeOffset(2026, 10, 27, 18, 0, 0, TimeSpan.Zero);
        var service = new TransactionImportService(env.Db, [env.Source],
            NullLogger<TransactionImportService>.Instance, new FixedClock(now));
        env.Source.Transactions = [CreateTransaction(
            transactionTime: now.UtcDateTime.AddSeconds(-ageSeconds))];
        await service.ImportAsync();
        env.Source.Transactions = [];

        var result = await service.ImportAsync();

        Assert.Equal(revoked ? 1 : 0, result.RevokedCount);
        Assert.Equal(revoked ? TransactionStatus.Revoked : TransactionStatus.Active,
            (await env.Db.Transactions.SingleAsync()).Status);
        Assert.Equal(revoked ? 1 : 0, await env.Db.TransactionAudits.CountAsync());
    }

    [Fact]
    public async Task Import_SourceMismatch_RejectsSnapshotAndRecordsFailure()
    {
        await using var env = await CreateEnvironmentAsync();
        env.Source.Transactions = [CreateTransaction(source: "Online")];

        await Assert.ThrowsAsync<InvalidOperationException>(() => env.Service.ImportAsync());

        Assert.Empty(await env.Db.Transactions.ToListAsync());
        Assert.Empty(await env.Db.TransactionAudits.ToListAsync());
        Assert.Equal("Failed", (await env.Db.ImportBatches.SingleAsync()).Status);
    }

    [Fact]
    public async Task Import_RepeatedSnapshot_PersistsUnchangedCountForEachExecution()
    {
        await using var env = await CreateEnvironmentAsync();
        env.Source.Transactions = [CreateTransaction()];
        await env.Service.ImportAsync();
        await env.Service.ImportAsync();
        await env.Service.ImportAsync();

        var batches = await env.Db.ImportBatches.OrderBy(b => b.Id).ToListAsync();
        Assert.Equal(3, batches.Count);
        Assert.Equal(1, batches[0].InsertedCount);
        Assert.Equal(0, batches[0].UnchangedCount);
        Assert.All(batches.Skip(1), b =>
        {
            Assert.Equal("Completed", b.Status);
            Assert.Equal(1, b.UnchangedCount);
            Assert.Equal(0, b.InsertedCount);
            Assert.NotNull(b.CompletedAt);
        });
        Assert.Single(await env.Db.Transactions.ToListAsync());
        Assert.Empty(await env.Db.TransactionAudits.ToListAsync());
    }

    [Fact]
    public async Task Import_SourceFailure_PreservesExistingTransactionsAndAudits()
    {
        await using var env = await CreateEnvironmentAsync();
        var original = CreateTransaction();
        env.Source.Transactions = [original];
        await env.Service.ImportAsync();
        env.Source.Transactions = [CreateTransaction(
            amount: 33.99m, transactionTime: original.TransactionTime)];
        await env.Service.ImportAsync();
        env.Source.ShouldFail = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => env.Service.ImportAsync());

        Assert.Equal(33.99m, (await env.Db.Transactions.SingleAsync()).Amount);
        Assert.Single(await env.Db.TransactionAudits.ToListAsync());
        Assert.Equal("Failed", (await env.Db.ImportBatches.OrderByDescending(b => b.Id)
            .FirstAsync()).Status);
    }
}
