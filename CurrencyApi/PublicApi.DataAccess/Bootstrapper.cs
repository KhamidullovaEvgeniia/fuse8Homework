using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PublicApi.DataAccess.Interfaces;
using PublicApi.DataAccess.Repositories;

namespace PublicApi.DataAccess;

public static class Bootstrapper
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, string? connectionString)
    {
        return services
            .AddDbContext<FavoriteCurrencyDbContext>(
                options =>
                {
                    options
                        .UseNpgsql(
                            connectionString,
                            npgsqlOptionsAction: sqlOptionsBuilder => { sqlOptionsBuilder.EnableRetryOnFailure(); })
                        .UseSnakeCaseNamingConvention();
                })
            .AddScoped<IFavoriteCurrencyRateRepository, FavoriteCurrencyRateRepository>();
    }
}