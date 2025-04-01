using System.Net;
using System.Text.Json;
using Fuse8.BackendInternship.PublicApi.Exceptions;
using Fuse8.BackendInternship.PublicApi.Interfaces;
using Fuse8.BackendInternship.PublicApi.Models;
using Fuse8.BackendInternship.PublicApi.Responses;
using Fuse8.BackendInternship.PublicApi.Settings;
using InternalApi;
using Microsoft.Extensions.Options;

namespace Fuse8.BackendInternship.PublicApi.Services;

public class CurrencyApiService : ICurrencyApiService
{
    private readonly ICurrencyHttpApi _currencyHttpApi;

    private readonly CurrencySetting _currencySetting;
    
    private readonly CurrencyApi.CurrencyApiClient _currencyApiClient;

    public CurrencyApiService(ICurrencyHttpApi currencyHttpApi, IOptionsSnapshot<CurrencySetting> currencySetting, CurrencyApi.CurrencyApiClient currencyApiClient)
    {
        _currencyHttpApi = currencyHttpApi;
        _currencyApiClient = currencyApiClient;
        _currencySetting = currencySetting.Value;
    }

    public async Task<CurrencyRate> GetCurrencyRateAsync(string currencyCode)
    {
        var request = new CurrencyRateRequest
        {
            CurrencyCode = currencyCode
        };

        // Отправляем запрос к gRPC-серверу
        var grpcResponse = await _currencyApiClient.GetCurrencyRateAsync(request);

        // Преобразуем результат из gRPC-ответа в свой объект ответа
        var response = new CurrencyRate
        {
            Code = grpcResponse.CurrencyCode,
            Value = (decimal)grpcResponse.Value
        };

        return response;
        
        // var result = await _currencyHttpApi.GetCurrencyRateAsync(currencyCode);
        //
        // var resultFirst = result.Data.First().Value;
        // var currencyRate = new CurrencyRate() { Code = resultFirst.Code, Value = RoundCurrencyValue(resultFirst.Value) };
        //
        // return currencyRate;
    }

    public async Task<DatedCurrencyRate> GetCurrencyDataWithRateAsync(string currencyCode, DateOnly date)
    {
        // Создаем запрос для gRPC
        var request = new CurrencyRateOnDateRequest
        {
            CurrencyCode = currencyCode,
            Date = date.ToString("yyyy-MM-dd") // Преобразуем дату в строку
        };

        // Выполняем gRPC запрос
        var response = await _currencyApiClient.GetCurrencyDataWithRateAsync(request);
        

        // Формируем результат
        var datedCurrencyRate = new DatedCurrencyRate()
        {
            Date = date,
            Code = response.CurrencyCode,
            Value = RoundCurrencyValue((decimal)response.Value) // Округляем значение курса валюты
        };

        return datedCurrencyRate;
        
        // var result = await _currencyHttpApi.GetCurrencyDataWithRateAsync(currencyCode, date);
        // var resultFirst = result.Data.First().Value;
        //
        // var datedCurrencyRate = new DatedCurrencyRate()
        // {
        //     Date = date, Code = resultFirst.Code, Value = RoundCurrencyValue(resultFirst.Value)
        // };
        //
        // return datedCurrencyRate;
    }

    public async Task<ApiSettings> GetApiSettingsAsync()
    {
        // Получаем настройки через gRPC
        var request = new ApiSettingsRequest(); // Если необходимо передать параметры, добавьте их в запрос
        var response = await _currencyApiClient.GetApiSettingsAsync(request);

        // Формируем объект ApiSettings
        var settingsApi = new ApiSettings()
        {
            DefaultCurrency = _currencySetting.Currency, // Берем валюту по умолчанию из конфигурации
            BaseCurrency = response.BaseCurrency, // Базовая валюта, полученная через gRPC
            NewRequestsAvailable = response.HasRequestsLeft, // Проверка на доступность новых запросов
            CurrencyRoundCount = _currencySetting.Accuracy // Количество знаков после запятой из конфигурации
        };

        return settingsApi;
        // var result = await _currencyHttpApi.GetApiQuotasAsync();
        //
        // var settingsApi = new ApiSettings()
        // {
        //     DefaultCurrency = _currencySetting.Currency,
        //     BaseCurrency = _currencySetting.BaseCurrency,
        //     RequestLimit = result.Quotas.Month.Total,
        //     RequestCount = result.Quotas.Month.Used,
        //     CurrencyRoundCount = _currencySetting.Accuracy
        // };
        //
        // return settingsApi;
    }

    private decimal RoundCurrencyValue(decimal value) => Math.Round(value, _currencySetting.Accuracy);
}