using Fuse8.BackendInternship.PublicApi;
using Microsoft.AspNetCore;
using Serilog;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.Filters;

var webHost = Host
	.CreateDefaultBuilder(args)
	.ConfigureWebHostDefaults(webHostBuilder => webHostBuilder.UseStartup<Startup>())
	.UseSerilog((context, _, loggerConfiguration) => loggerConfiguration
		.ReadFrom.Configuration(context.Configuration)
		.Enrich.FromLogContext()

		// Добавляем обработчики для корректного логирования исключений.
		// Если случится исключение, для которого не зарегистрирован обработчик, будет использован обработчик по умолчанию.
		// Serilog рекурсивно пройдёт по всем публичным свойствам исключения и добавит их в сообщение лога.
		.Enrich.WithExceptionDetails(
			new DestructuringOptionsBuilder()
				// Для основных типов исключений есть стандартные обработчики в библиотеке Serilog
				.WithDefaultDestructurers()
                    
				// Уберем из детального логирования некоторые поля
				.WithFilter(
					new IgnorePropertyByNameExceptionFilter(
						nameof(Exception.StackTrace), // Эти данные и так записываются встроенным обработчиком исключения
						nameof(Exception.Message), // Текст сообщения можно посмотреть в StackTrace
						nameof(Exception.TargetSite), // Бесполезное свойство. В асинхронном коде чаще всего будет записано "Void MoveNext()"
						nameof(Exception.Source), // Имя приложения (проекта) можно понять по StackTrace
						nameof(Exception.HResult) // Бесполезное свойство. Конкретную ошибку можно понять по ее типу и тексту ошибки. Также это свойство и так записывается в Elastic встроенным обработчиком исключения "Type"
					)
				)
		))
	.Build();

await webHost.RunAsync();