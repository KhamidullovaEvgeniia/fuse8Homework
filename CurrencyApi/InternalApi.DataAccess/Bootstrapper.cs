using InternalApi.DataAccess.Interfaces;
using InternalApi.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InternalApi.DataAccess;

public static class Bootstrapper
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, string? connectionString)
    {
        return services.AddDbContext<CurrencyDbContext>(
            options =>
            {
                options
                    .UseNpgsql(
                        connectionString,
                        npgsqlOptionsAction: sqlOptionsBuilder => { sqlOptionsBuilder.EnableRetryOnFailure(); })
                    .UseSnakeCaseNamingConvention();
            })
            .AddScoped<IExchangeDateRepository, ExchangeDateRepository>()
            .AddScoped<ICurrencyRateRepository, CurrencyRateRepository>();
    }
}