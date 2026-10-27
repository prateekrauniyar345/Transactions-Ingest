using Microsoft.EntityFrameworkCore;
using TransactionsIngest.Api.Models;

namespace TransactionsIngest.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var transaction = modelBuilder.Entity<Transaction>();

        transaction.ToTable("Transactions");

        transaction.HasKey(t => t.Id);

        transaction.HasIndex(t => new
        {
            t.SourceSystem,
            t.TransactionId
        }).IsUnique();

        transaction.Property(t => t.SourceSystem)
            .IsRequired()
            .HasMaxLength(19);

        transaction.Property(t => t.Status)
            .HasConversion<string>()
            .IsRequired();
    }
}