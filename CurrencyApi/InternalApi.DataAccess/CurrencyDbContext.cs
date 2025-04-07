using InternalApi.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace InternalApi.DataAccess;

public class CurrencyDbContext: DbContext
{
    public DbSet<ExchangeDate> ExchangeDates { get; set; }
    public DbSet<CurrencyRate> CurrencyRates { get; set; }

    public CurrencyDbContext(DbContextOptions<CurrencyDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("cur");
        
        modelBuilder.Entity<ExchangeDate>()
            .HasKey(e => e.Id);

        modelBuilder.Entity<CurrencyRate>()
            .HasKey(cr => new { cr.Currency, cr.Date }); 
        modelBuilder.Entity<CurrencyRate>()
            .HasOne(cr => cr.ExchangeDate)
            .WithMany(ed => ed.CurrencyRates)
            .HasForeignKey(cr => cr.Date)
            .HasPrincipalKey(ed => ed.Date); 
    }
}