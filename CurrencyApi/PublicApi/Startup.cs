using System.Text.Json.Serialization;
using Fuse8.BackendInternship.PublicApi.Filters;
using Fuse8.BackendInternship.PublicApi.Interfaces;
using Fuse8.BackendInternship.PublicApi.Middlewares;
using Fuse8.BackendInternship.PublicApi.Services;
using Fuse8.BackendInternship.PublicApi.Settings;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

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
            .AddControllers(options => { options.Filters.Add<ExceptionFilter>(); })

            // Добавляем глобальные настройки для преобразования Json
            .AddJsonOptions(
                options =>
                {
                    // Добавляем конвертер для енама
                    // По умолчанию енам преобразуется в цифровое значение
                    // Этим конвертером задаем перевод в строковое значение
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(
            c =>
            {
                c.SwaggerDoc(
                    "v1",
                    new OpenApiInfo()
                    {
                        Title = "API",
                        Version = "v1",
                        Description = "Api"
                    });

                c.IncludeXmlComments(
                    Path.Combine(AppContext.BaseDirectory, $"{typeof(Program).Assembly.GetName().Name}.xml"),
                    true);
            });

        services.Configure<CurrencyApiSettings>(_configuration.GetSection("CurrencyApiSettings"));
        services.Configure<CurrencySetting>(_configuration.GetSection("CurrencySetting"));

        services.AddScoped<ICurrencyApiService, CurrencyApiService>();
        services.AddTransient<LoggingHandler>();

        services
            .AddHttpClient<ICurrencyApiService, CurrencyApiService>()
            .ConfigureHttpClient(
                (provider, client) =>
                {
                    var settings = provider.GetRequiredService<IOptions<CurrencyApiSettings>>().Value;
                    settings.Validate();

                    client.BaseAddress = new Uri(settings.BaseUrl);
                    client.DefaultRequestHeaders.Add("apikey", settings.ApiKey);
                })
            .AddHttpMessageHandler<LoggingHandler>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseRouting().UseEndpoints(endpoints => endpoints.MapControllers());
    }
}