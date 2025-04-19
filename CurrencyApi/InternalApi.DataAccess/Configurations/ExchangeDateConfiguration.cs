using InternalApi.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternalApi.DataAccess.Configurations;

public class ExchangeDateConfiguration : IEntityTypeConfiguration<ExchangeDate>
{
    public void Configure(EntityTypeBuilder<ExchangeDate> builder)
    {
        builder.HasKey(e => e.Id);
    }
}