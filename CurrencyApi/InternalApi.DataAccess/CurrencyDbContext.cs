using InternalApi.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace InternalApi.DataAccess;

public class CurrencyDbContext : DbContext
{
    public DbSet<ExchangeDate> ExchangeDates { get; set; }

    public DbSet<CurrencyRate> CurrencyRates { get; set; }

    public const string SchemaName = "cur";

    public CurrencyDbContext(DbContextOptions<CurrencyDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CurrencyDbContext).Assembly);
    }
}