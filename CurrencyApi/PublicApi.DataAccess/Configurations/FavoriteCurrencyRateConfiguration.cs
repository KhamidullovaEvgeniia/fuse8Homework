using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PublicApi.DataAccess.Models;

namespace PublicApi.DataAccess.Configurations;

public class FavoriteCurrencyRateConfiguration : IEntityTypeConfiguration<FavoriteCurrencyRate>
{
    public void Configure(EntityTypeBuilder<FavoriteCurrencyRate> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasIndex(x => x.Name).IsUnique();

        builder.HasIndex(x => new { x.Currency, x.BaseCurrency }).IsUnique();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.BaseCurrency).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Currency).IsRequired().HasMaxLength(100);

        builder.ToTable(t => t.HasCheckConstraint("CK_Currency_NotEqual", "\"currency\" <> \"base_currency\""));
    }
}