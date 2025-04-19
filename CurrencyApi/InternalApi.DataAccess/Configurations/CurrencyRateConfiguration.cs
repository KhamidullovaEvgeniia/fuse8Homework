using InternalApi.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternalApi.DataAccess.Configurations;

public class CurrencyRateConfiguration : IEntityTypeConfiguration<CurrencyRate>
{
    public void Configure(EntityTypeBuilder<CurrencyRate> builder)
    {
        builder.HasKey(cr => new { cr.Currency, cr.DateId });

        builder
            .HasOne(cr => cr.ExchangeDate)
            .WithMany(ed => ed.CurrencyRates)
            .HasForeignKey(cr => cr.DateId)
            .HasPrincipalKey(ed => ed.Id);
    }
}