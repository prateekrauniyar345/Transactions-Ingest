using Microsoft.EntityFrameworkCore;
using TransactionsIngest.Api.Models;

namespace TransactionsIngest.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    // get the DbSet for transactions and transaction audits
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<TransactionAudit> TransactionAudits
    => Set<TransactionAudit>();

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
    }
}
