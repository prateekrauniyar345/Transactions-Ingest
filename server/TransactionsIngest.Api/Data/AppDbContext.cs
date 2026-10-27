using Microsoft.EntityFrameworkCore;
using TransactionsIngest.Api.Models;

namespace TransactionsIngest.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    // get the DbSet for transactions 
    public DbSet<Transaction> Transactions => Set<Transaction>();

    // get the DbSet for TransactionAudits
    public DbSet<TransactionAudit> TransactionAudits
    => Set<TransactionAudit>();

    // get the DbSet for ImportBatches
    public DbSet<ImportBatch> ImportBatches => Set<ImportBatch>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // references the Transaction Table
        var transaction = modelBuilder.Entity<Transaction>();

        transaction.ToTable("Transactions");
        // primary key for the Transaction table
        transaction.HasKey(t => t.Id);
        transaction.HasIndex(t => new
        {
            t.SourceSystem,
            t.TransactionId
        }).IsUnique();
        transaction.Property(t => t.SourceSystem)
            .IsRequired()
            .HasMaxLength(19);
        transaction.Property(t => t.CardNumber)
            .IsRequired()
            .HasMaxLength(19);
        transaction.Property(t => t.LocationCode)
            .IsRequired()
            .HasMaxLength(19);
        transaction.Property(t => t.ProductName)
            .IsRequired()
            .HasMaxLength(19);
        transaction.Property(t => t.Status)
            .HasConversion<string>()
            .IsRequired();



        // references the TransactionAudit Table
        var audit = modelBuilder.Entity<TransactionAudit>();
        audit.ToTable("TransactionAudits");
        // primary key for the TransactionAudit table
        audit.HasKey(a => a.Id);
        audit.Property(a => a.ChangeType)
            .IsRequired()
            .HasMaxLength(30);
        audit.Property(a => a.FieldName)
            .IsRequired()
            .HasMaxLength(50);
        audit.HasOne(a => a.Transaction)
            .WithMany(t => t.AuditHistory)
            .HasForeignKey(a => a.TransactionRecordId)
            .OnDelete(DeleteBehavior.Restrict);
        audit.HasIndex(a => new
        {
            a.TransactionRecordId,
            a.ChangedAt
        });



        // references the ImportBatch Table
        var batch = modelBuilder.Entity<ImportBatch>();
        batch.ToTable("ImportBatches");
        // primary key for the ImportBatch table
        batch.HasKey(b => b.Id);
        batch.Property(b => b.StartedAt)
            .IsRequired();
        batch.Property(b => b.CompletedAt)
            .IsRequired(false);
        batch.Property(b => b.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("Running");
        batch.Property(b => b.InsertedCount)
            .HasDefaultValue(0);
        batch.Property(b => b.UpdatedCount)
            .HasDefaultValue(0);
        batch.Property(b => b.RevokedCount)
            .HasDefaultValue(0);
        batch.Property(b => b.UnchangedCount)
            .HasDefaultValue(0);
    }
}
