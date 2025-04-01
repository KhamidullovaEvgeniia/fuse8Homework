using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Fuse8.BackendInternship.InternalApi.Contracts;
using InternalApi.Binders;
using InternalApi.Filters;
using InternalApi.Interfaces;
using InternalApi.JsonConvectors;
using InternalApi.Middlewares;
using InternalApi.Services;
using InternalApi.Settings;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;

namespace InternalApi;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddControllers(
                options =>
                {
                    options.Filters.Add<ExceptionFilter>();
                    options.ModelBinderProviders.Insert(0, new DateOnlyBinderProvider());
                })
        
            // Добавляем глобальные настройки для преобразования Json
            .AddJsonOptions(
                options =>
                {
                    // Добавляем конвертер для енама
                    // По умолчанию енам преобразуется в цифровое значение
                    // Этим конвертером задаем перевод в строковое значение
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConvector());
                });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(
            c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo() { Title = "API", Version = "v1", Description = "Api" });

                c.IncludeXmlComments(
                    Path.Combine(AppContext.BaseDirectory, $"{typeof(Program).Assembly.GetName().Name}.xml"),
                    true);
            });

        services.AddGrpc();
        
        services.Configure<CurrencyApiSettings>(_configuration.GetSection("CurrencyApiSettings"));
        services.Configure<CurrencySetting>(_configuration.GetSection("CurrencySetting"));

        services.AddScoped<ICurrencyApiService, CurrencyApiService>();
        services.AddScoped<ICurrencyHttpApi, CurrencyHttpApi>();
        services.AddScoped<ICachedCurrencyAPI, CachedCurrencyService>();
        services.AddScoped<ICurrencyAPI, CurrencyApiService>();
        services.AddTransient<LoggingHandler>();
        
        //services.AddGrpc();

        services
            .AddHttpClient<ICurrencyHttpApi, CurrencyHttpApi>()
            .AddPolicyHandler(
                HttpPolicyExtensions
                    // Настраиваем повторный запрос при получении ошибок сервера (HTTP-код = 5XX) и для таймаута выполнения запроса (HTTP-код = 408)
                    .HandleTransientHttpError()
                    .WaitAndRetryAsync(
                        retryCount: 3,
                        sleepDurationProvider: retryAttempt =>
                        {
                            // Настраиваем экспоненциальную задержку для отправки повторного запроса при ошибке
                            // 1-я попытка будет выполнена через 1 сек
                            // 2-я - через 3 сек
                            // 3-я - через 7 сек
                            return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) - 1);
                        }))
            .ConfigureHttpClient(
                (provider, client) =>
                {
                    var settings = provider.GetRequiredService<IOptions<CurrencyApiSettings>>().Value;

                    client.BaseAddress = new Uri(settings.BaseUrl);
                    client.DefaultRequestHeaders.Add("apikey", settings.ApiKey);
                })
            .AddHttpMessageHandler<LoggingHandler>();

        services
            .AddOptions<CurrencyApiSettings>()
            .Bind(_configuration.GetSection(CurrencyApiSettings.SectionName))

            // Настраиваем валидацию свойств по дата-атрибутам
            .ValidateDataAnnotations()

            // Настраиваем, чтобы валидация свойств была при старте приложения
            .ValidateOnStart();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(
                options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Currency API v1");
                    options.RoutePrefix = "";
                });
        }

        app.UseMiddleware<RequestLoggingMiddleware>();

        app.UseWhen(
            predicate: context => context.Connection.LocalPort == _configuration.GetValue<int>("GrpcPort"),
            configuration: grpcBuilder =>
            {
                grpcBuilder.UseRouting();
                grpcBuilder.UseEndpoints(endpoints => endpoints.MapGrpcService<GrpcService>());
            });
        
        // app.UseRouting();
        // app.UseEndpoints(endpoints => endpoints.MapGrpcService<GrpcService>());
        
        app.UseRouting().UseEndpoints(endpoints => endpoints.MapControllers());
    }
}