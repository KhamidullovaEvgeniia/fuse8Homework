using System.Text.Json.Serialization;
using Audit.Core;
using Audit.Http;
using General.Binders;
using General.Filters;
using General.JsonConvectors;
using General.Middlewares;
using InternalApi.DataAccess;
using InternalApi.Interfaces;
using InternalApi.Services;
using InternalApi.Settings;
using InterpolatedParsing;
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

        services.AddScoped<ICachedCurrencyAPI, CachedCurrencyService>();
        services.AddScoped<ICurrencyAPI, CurrencyApiService>();

        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__CurrencyDb")
                               ?? _configuration.GetConnectionString("CurrencyDb");

        services.AddDataAccess(connectionString);

        services.AddGrpc();

        Configuration.Setup().UseSerilog();

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
                    using (var scope = provider.CreateScope())
                    {
                        var scopedProvider = scope.ServiceProvider;
                        var settings = scopedProvider.GetRequiredService<IOptionsSnapshot<CurrencyApiSettings>>().Value;
                        client.BaseAddress = new Uri(settings.BaseUrl);
                        client.DefaultRequestHeaders.Add("apikey", settings.ApiKey);
                    }
                })
            .AddAuditHandler(
                audit => audit
                    .IncludeRequestBody()
                    .IncludeResponseBody()
                    .IncludeContentHeaders()
                    .IncludeRequestHeaders()
                    .IncludeResponseHeaders());
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        var (grpcPort, webApiPort) = GetPorts();

        // Настраиваем доступ к webApi только по webApi-порту
        app.UseWhen(
            predicate: context => context.Connection.LocalPort == webApiPort,
            configuration: apiBuilder => ConfigureWebApi(apiBuilder, env));

        // Настраиваем доступ к grpc-сервису только по grpc-порту
        app.UseWhen(predicate: context => context.Connection.LocalPort == grpcPort, configuration: ConfigureGrpc);
    }

    private (int GrpcPort, int WebApiPort) GetPorts()
    {
        // var grpcPortFromEnv = Environment.GetEnvironmentVariable("GRPC_PORT");
        // var webApiPortFromEnv = Environment.GetEnvironmentVariable("REST_PORT");
        //
        // if (!string.IsNullOrWhiteSpace(grpcPortFromEnv) && !string.IsNullOrWhiteSpace(webApiPortFromEnv))
        // {
        //     var parsedGrpcPort = int.Parse(grpcPortFromEnv);
        //     var parsedWebApiPort = int.Parse(webApiPortFromEnv);
        //     return (parsedGrpcPort, parsedWebApiPort);
        // }

        var grpcPort = ParsePortFromEndpoint("gRPC");
        var webApiPort = ParsePortFromEndpoint("WebApi");

        return (grpcPort, webApiPort);

        int ParsePortFromEndpoint(string endpointKey)
        {
            // Получаем из конфигурации приложения endpoint, предназначенный для запрошенного сервиса
            var endpointUrl = _configuration.GetValue<string>($"Kestrel:Endpoints:{endpointKey}:Url")
                              ?? throw new InvalidOperationException($"Не найден Endpoint по ключу '{endpointKey}'");

            // Получаем порт
            // Урл может быть сконфигурирован в виде http://localhost:80, http://127.0.0.1:80, либо http://+:80.
            // Получаем значение порта с помощью библиотеки InterpolatedParser (аналог Regex только более читаемый)
            var schema = string.Empty;
            var host = string.Empty;
            var port = 0;
            InterpolatedParser.Parse(endpointUrl, $"{schema}://{host}:{port}");

            return port;
        }
    }

    private static void ConfigureWebApi(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app
                .UseSwagger()
                .UseSwaggerUI(
                    options =>
                    {
                        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Currency API v1");
                        options.RoutePrefix = "";
                    });
        }

        app.UseRouting().UseMiddleware<RequestLoggingMiddleware>().UseEndpoints(endpoints => endpoints.MapControllers());
    }

    private static void ConfigureGrpc(IApplicationBuilder app) =>
        app.UseRouting().UseEndpoints(x => x.MapGrpcService<GrpcService>());
}