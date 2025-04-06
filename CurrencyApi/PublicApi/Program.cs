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
		.ReadFrom.Configuration(context.Configuration))
	.Build();

await webHost.RunAsync();