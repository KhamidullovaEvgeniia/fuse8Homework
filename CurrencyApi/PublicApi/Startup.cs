using System.Text.Json.Serialization;
using Audit.Core;
using Audit.Http;
using Currency;
using General.Binders;
using General.Filters;
using General.JsonConvectors;
using General.Middlewares;
using Fuse8.BackendInternship.PublicApi.Interfaces;
using Fuse8.BackendInternship.PublicApi.Services;
using Fuse8.BackendInternship.PublicApi.Settings;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using PublicApi.DataAccess;

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
        services.AddScoped<IFavoriteCurrencyService, FavoriteCurrencyService>();
        
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__CurrencyDb") 
                               ?? _configuration.GetConnectionString("CurrencyDb");
        services.AddDataAccess(connectionString);
        
        Configuration.Setup().UseSerilog();

        services
            .AddGrpcClient<CurrencyApi.CurrencyApiClient>(
                (provider, options) =>
                {
                    using (var scope = provider.CreateScope())
                    {
                        var scopedProvider = scope.ServiceProvider;
                        var settings = scopedProvider.GetRequiredService<IOptionsSnapshot<CurrencyApiSettings>>().Value;
                        var grpcUrl = Environment.GetEnvironmentVariable("InternalApi__GrpcEndpoint") 
                                      ?? settings.GrpcUrl;
                        options.Address = new Uri(grpcUrl);
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