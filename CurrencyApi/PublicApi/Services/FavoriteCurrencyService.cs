using Currency;
using Fuse8.BackendInternship.PublicApi.Interfaces;
using Fuse8.BackendInternship.PublicApi.Models;
using Fuse8.BackendInternship.PublicApi.Settings;
using Microsoft.Extensions.Options;
using PublicApi.DataAccess.Interfaces;
using PublicApi.DataAccess.Models;

namespace Fuse8.BackendInternship.PublicApi.Services;

public class FavoriteCurrencyService : IFavoriteCurrencyService
{
    private readonly CurrencySetting _currencySetting;

    private readonly CurrencyApi.CurrencyApiClient _currencyApiClient;

    private readonly IFavoriteCurrencyRateRepository _repository;

    public FavoriteCurrencyService(
        IOptionsSnapshot<CurrencySetting> currencySetting,
        CurrencyApi.CurrencyApiClient currencyApiClient,
        IFavoriteCurrencyRateRepository repository)
    {
        _currencyApiClient = currencyApiClient;
        _currencySetting = currencySetting.Value;
        _repository = repository;
    }

    public async Task<FavoriteCurrencyRateDTO> GetFavoriteCurrencyRateByNameAsync(string name)
    {
        var currencyRate = await _repository.GetByNameAsync(name);
        if (currencyRate is null)
            throw new KeyNotFoundException();

        return new FavoriteCurrencyRateDTO
        {
            Name = currencyRate.Name,
            Currency = currencyRate.Currency,
            BaseCurrency = currencyRate.BaseCurrency
        };
    }

    public async Task<FavoriteCurrencyRateDTO[]> GetAllFavoriteCurrencyRatesAsync()
    {
        var currencyRates = await _repository.GetAllAsync();
        if (currencyRates is null)
            throw new KeyNotFoundException();

        return currencyRates.Select(
                x => new FavoriteCurrencyRateDTO
                {
                    Name = x.Name,
                    Currency = x.Currency,
                    BaseCurrency = x.BaseCurrency
                })
            .ToArray();
    }

    public async Task AddFavoriteCurrencyRateAsync(FavoriteCurrencyRateDTO currencyRateDTO)
    {
        if (await _repository.ExistsByNameAsync(currencyRateDTO.Name))
        {
            throw new InvalidOperationException("Курс с таким именем уже существует.");
        }

        if (await _repository.ExistsByCurrenciesAsync(currencyRateDTO.Currency, currencyRateDTO.BaseCurrency))
        {
            throw new InvalidOperationException("Такой курс с данной валютной парой уже существует.");
        }
        
        var currencyRate = new FavoriteCurrencyRate
        {
            Name = currencyRateDTO.Name,
            Currency = currencyRateDTO.Currency,
            BaseCurrency = currencyRateDTO.BaseCurrency
        };
        
        await _repository.AddAsync(currencyRate);
    }

    public async Task UpdateFavoriteCurrencyRateAsync(FavoriteCurrencyRateDTO currencyRateDTO)
    {
        if (await _repository.ExistsByCurrenciesAsync(currencyRateDTO.Currency, currencyRateDTO.BaseCurrency))
        {
            throw new InvalidOperationException("Такой курс с данной валютной парой уже существует.");
        }

        if (!await _repository.ExistsByNameAsync(currencyRateDTO.Name))
        {
            throw new InvalidOperationException("Курс с таким именем не существует.");
        }
        
        var currencyRate = new FavoriteCurrencyRate
        {
            Name = currencyRateDTO.Name,
            Currency = currencyRateDTO.Currency,
            BaseCurrency = currencyRateDTO.BaseCurrency
        };
        
        await _repository.UpdateByNameAsync(currencyRate);
    }

    public async Task DeleteFavoriteCurrencyRateByNameAsync(string name)
    {
        await _repository.DeleteAsync(name);
    }
}