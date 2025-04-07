using System.Net;
using System.Text.Json;
using Currency;
using Fuse8.BackendInternship.PublicApi.Exceptions;
using Fuse8.BackendInternship.PublicApi.Interfaces;
using Fuse8.BackendInternship.PublicApi.Models;
using Fuse8.BackendInternship.PublicApi.Responses;
using Fuse8.BackendInternship.PublicApi.Settings;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Options;

namespace Fuse8.BackendInternship.PublicApi.Services;

public class CurrencyApiService : ICurrencyApiService
{
    private readonly CurrencySetting _currencySetting;

    private readonly CurrencyApi.CurrencyApiClient _currencyApiClient;

    public CurrencyApiService(
        IOptionsSnapshot<CurrencySetting> currencySetting,
        CurrencyApi.CurrencyApiClient currencyApiClient)
    {
        _currencyApiClient = currencyApiClient;
        _currencySetting = currencySetting.Value;
    }

    public async Task<CurrencyRate> GetCurrencyRateAsync(string currencyCode)
    {
        var request = new CurrencyRateRequest { CurrencyCode = currencyCode };

        // Отправляем запрос к gRPC-серверу
        // TODO:  сладй 54-55
        var grpcResponse = await _currencyApiClient.GetCurrencyRateAsync(request);

        // Преобразуем результат из gRPC-ответа в свой объект ответа
        var response = new CurrencyRate
        {
            Code = grpcResponse.CurrencyCode,
            Value = RoundCurrencyValue(grpcResponse.Value)
        };

        return response;
    }

    public async Task<DatedCurrencyRate> GetCurrencyDataWithRateAsync(string currencyCode, DateOnly date)
    {
        // Создаем запрос для gRPC
        var request = new CurrencyRateOnDateRequest
        {
            CurrencyCode = currencyCode,
            Date = new GRPCDateOnly
            {
                Day = date.Day,
                Month = date.Month,
                Year = date.Year
            } // Преобразуем дату в строку
        };

        // Выполняем gRPC запрос
        var response = await _currencyApiClient.GetCurrencyDataWithRateAsync(request);

        // Формируем результат
        var datedCurrencyRate = new DatedCurrencyRate()
        {
            Date = date,
            Code = response.CurrencyCode,
            Value = RoundCurrencyValue(response.Value) // Округляем значение курса валюты
        };

        return datedCurrencyRate;
    }

    public async Task<ApiSettings> GetApiSettingsAsync(Empty request)
    {
        // Получаем настройки через gRPC
        var response = await _currencyApiClient.GetApiSettingsAsync(request);

        // Формируем объект ApiSettings
        var settingsApi = new ApiSettings()
        {
            DefaultCurrency = _currencySetting.Currency, // Берем валюту по умолчанию из конфигурации
            NewRequestsAvailable = response.HasRequestsLeft, // Проверка на доступность новых запросов
            CurrencyRoundCount = _currencySetting.Accuracy // Количество знаков после запятой из конфигурации
        };

        return settingsApi;
    }

    private decimal RoundCurrencyValue(double value) => Math.Round((decimal)value, _currencySetting.Accuracy);
}