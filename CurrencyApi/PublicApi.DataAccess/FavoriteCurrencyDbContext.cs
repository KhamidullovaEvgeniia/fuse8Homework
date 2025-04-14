using Microsoft.EntityFrameworkCore;
using PublicApi.DataAccess.Models;

namespace PublicApi.DataAccess;

public class FavoriteCurrencyDbContext : DbContext
{
    public DbSet<FavoriteCurrencyRate> FavoriteCurrencyRates { get; set; }

    public const string SchemaName = "user";

    public FavoriteCurrencyDbContext(DbContextOptions<FavoriteCurrencyDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FavoriteCurrencyDbContext).Assembly);
    }
}