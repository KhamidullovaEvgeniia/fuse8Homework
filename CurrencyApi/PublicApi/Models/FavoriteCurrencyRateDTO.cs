namespace Fuse8.BackendInternship.PublicApi.Models;

public sealed class FavoriteCurrencyRateDTO
{
    public required string Name { get; init; }

    public required string Currency { get; init; }

    public required string BaseCurrency { get; init; }
}