using System.Text.Json.Serialization;
using Audit.Http;
using Currency;
using Fuse8.BackendInternship.PublicApi.Binders;
using Fuse8.BackendInternship.PublicApi.Filters;
using Fuse8.BackendInternship.PublicApi.Interfaces;
using Fuse8.BackendInternship.PublicApi.Middlewares;
using Fuse8.BackendInternship.PublicApi.Services;
using Fuse8.BackendInternship.PublicApi.Settings;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;

namespace Fuse8.BackendInternship.PublicApi;

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

        services.AddScoped<ICurrencyApiService, CurrencyApiService>();
        services.AddScoped<ICurrencyHttpApi, CurrencyHttpApi>();
        services.AddTransient<LoggingHandler>();

        // services
        //     .AddHttpClient<ICurrencyHttpApi, CurrencyHttpApi>()
        //     .AddPolicyHandler(
        //         HttpPolicyExtensions
        //
        //             // Настраиваем повторный запрос при получении ошибок сервера (HTTP-код = 5XX) и для таймаута выполнения запроса (HTTP-код = 408)
        //             .HandleTransientHttpError()
        //             .WaitAndRetryAsync(
        //                 retryCount: 3,
        //                 sleepDurationProvider: retryAttempt =>
        //                 {
        //                     // Настраиваем экспоненциальную задержку для отправки повторного запроса при ошибке
        //                     // 1-я попытка будет выполнена через 1 сек
        //                     // 2-я - через 3 сек
        //                     // 3-я - через 7 сек
        //                     return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) - 1);
        //                 }))
        //     .ConfigureHttpClient(
        //         (provider, client) =>
        //         {
        //             var settings = provider.GetRequiredService<IOptions<CurrencyApiSettings>>().Value;
        //
        //             client.BaseAddress = new Uri(settings.BaseUrl);
        //             client.DefaultRequestHeaders.Add("apikey", settings.ApiKey);
        //         })
        //     .AddHttpMessageHandler<LoggingHandler>();

        services
            .AddGrpcClient<CurrencyApi.CurrencyApiClient>(
                (provider, options) =>
                {
                    using (var scope = provider.CreateScope())
                    {
                        var scopedProvider = scope.ServiceProvider;
                        var settings = scopedProvider.GetRequiredService<IOptionsSnapshot<CurrencyApiSettings>>().Value;
                        options.Address = new Uri(settings.GrpcUrl);
                    }
                })
            .AddAuditHandler(
                audit => audit
                    .IncludeRequestBody()
                    .IncludeResponseBody()
                    .IncludeContentHeaders()
                    .IncludeRequestHeaders()
                    .IncludeResponseHeaders());

        services
            .AddOptions<CurrencyApiSettings>()
            .Bind(_configuration.GetSection(CurrencyApiSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<CurrencySetting>()
            .Bind(_configuration.GetSection(CurrencySetting.SectionName))
            .ValidateDataAnnotations()
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
        app.UseRouting().UseEndpoints(endpoints => endpoints.MapControllers());
    }
}