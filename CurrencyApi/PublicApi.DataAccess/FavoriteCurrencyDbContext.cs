using Microsoft.EntityFrameworkCore;
using PublicApi.DataAccess.Models;

namespace PublicApi.DataAccess;

public class FavoriteCurrencyDbContext : DbContext
{
    public FavoriteCurrencyDbContext(DbContextOptions<FavoriteCurrencyDbContext> options) : base(options)
    {
    }

    public DbSet<FavoriteCurrencyRate> FavoriteCurrencyRates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("user");

        modelBuilder.Entity<FavoriteCurrencyRate>(
            entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.HasIndex(x => x.Name).IsUnique();

                entity
                    .HasIndex(
                        x => new
                        {
                            x.Currency,
                            x.BaseCurrency
                        })
                    .IsUnique();

                entity.Property(x => x.Name).IsRequired().HasMaxLength(100);

                entity.HasCheckConstraint("CK_Currency_NotEqual", "\"currency\" <> \"base_currency\"");
            });
    }
}