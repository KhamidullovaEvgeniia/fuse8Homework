using InternalApi.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace InternalApi.DataAccess;

public class CurrencyDbContext: DbContext
{
    public DbSet<ExchangeDate> ExchangeDates { get; set; }
    public DbSet<CurrencyRate> CurrencyRates { get; set; }
    
    public const string SchemaName = "cur";

    public CurrencyDbContext(DbContextOptions<CurrencyDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);
        
        modelBuilder.Entity<ExchangeDate>()
            .HasKey(e => e.Id);

        modelBuilder.Entity<CurrencyRate>()
            .HasKey(cr => new { cr.Currency, cr.DateId }); 
        modelBuilder.Entity<CurrencyRate>()
            .HasOne(cr => cr.ExchangeDate)
            .WithMany(ed => ed.CurrencyRates)
            .HasForeignKey(cr => cr.DateId)
            .HasPrincipalKey(ed => ed.Id); 
    }
    
    public void BeginTransaction()
    {
        Database.BeginTransaction();
    }

    public void CommitTransaction()
    {
        Database.CommitTransaction();
    }

    public void RollbackTransaction()
    {
        Database.RollbackTransaction();
    }
}