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

    public async Task<FavoriteCurrencyRateDTO> GetFavoriteCurrencyRateByNameAsync(
        string name,
        CancellationToken cancellationToken)
    {
        var currencyRate = await _repository.GetByNameAsync(name);
        if (currencyRate is null)
            throw new InvalidOperationException($"Favorite currency {name} not found");

        return new FavoriteCurrencyRateDTO
        {
            Name = currencyRate.Name,
            Currency = currencyRate.Currency,
            BaseCurrency = currencyRate.BaseCurrency
        };
    }

    public async Task<FavoriteCurrencyRateDTO[]> GetAllFavoriteCurrencyRatesAsync(CancellationToken cancellationToken)
    {
        var currencyRates = await _repository.GetAllAsync();
        if (currencyRates is null)
            throw new InvalidOperationException("Favorite currency not found");

        return currencyRates
            .Select(
                x => new FavoriteCurrencyRateDTO
                {
                    Name = x.Name,
                    Currency = x.Currency,
                    BaseCurrency = x.BaseCurrency
                })
            .ToArray();
    }

    public async Task AddFavoriteCurrencyRateAsync(FavoriteCurrencyRateDTO currencyRateDTO, CancellationToken cancellationToken)
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

    public async Task UpdateFavoriteCurrencyRateAsync(
        FavoriteCurrencyRateDTO currencyRateDTO,
        CancellationToken cancellationToken)
    {
        if (await _repository.ExistsByCurrenciesAsync(currencyRateDTO.Currency, currencyRateDTO.BaseCurrency))
            throw new InvalidOperationException("Такой курс с данной валютной парой уже существует.");

        if (!await _repository.ExistsByNameAsync(currencyRateDTO.Name))
            throw new InvalidOperationException("Курс с таким именем не существует.");

        var currencyRate = new FavoriteCurrencyRate
        {
            Name = currencyRateDTO.Name,
            Currency = currencyRateDTO.Currency,
            BaseCurrency = currencyRateDTO.BaseCurrency
        };

        await _repository.UpdateByNameAsync(currencyRate);
    }

    public async Task DeleteFavoriteCurrencyRateByNameAsync(string name, CancellationToken cancellationToken)
    {
        await _repository.DeleteAsync(name);
    }

    public async Task<CurrencyRate> GetSelectedCurrencyRateByName(string name, CancellationToken cancellationToken)
    {
        var currencyRate = await _repository.GetByNameAsync(name);
        if (currencyRate is null)
            throw new KeyNotFoundException();

        var currency = currencyRate.Currency;
        var baseCurrency = currencyRate.BaseCurrency;
        var currencyRateRequest = new CurrencyRateRequest
        {
            BaseCurrencyCode = baseCurrency,
            CurrencyCode = currency,
        };

        var response = await _currencyApiClient.GetCurrencyRateAsync(currencyRateRequest);

        return new CurrencyRate
        {
            Code = response.CurrencyCode,
            Value = Helpers.CurrencyHelper.RoundCurrencyValue(response.Value, _currencySetting.Accuracy)
        };
    }

    public async Task<DatedCurrencyRate> GetSelectedCurrencyRateByDate(
        string name,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        var currencyRate = await _repository.GetByNameAsync(name);
        if (currencyRate is null)
            throw new KeyNotFoundException();

        var currency = currencyRate.Currency;
        var baseCurrency = currencyRate.BaseCurrency;
        var grpcDate = new GRPCDateOnly
        {
            Year = date.Year,
            Month = date.Month,
            Day = date.Day
        };

        var currencyRateRequest = new CurrencyRateOnDateRequest
        {
            BaseCurrencyCode = baseCurrency,
            CurrencyCode = currency,
            Date = grpcDate
        };

        var response = await _currencyApiClient.GetCurrencyDataWithRateAsync(currencyRateRequest);

        var dateOnly = new DateOnly(response.Date.Year, response.Date.Month, response.Date.Day);

        return new DatedCurrencyRate()
        {
            Code = response.CurrencyCode,
            Value = Helpers.CurrencyHelper.RoundCurrencyValue(response.Value, _currencySetting.Accuracy),
            Date = dateOnly
        };
    }
}