using Fuse8.BackendInternship.PublicApi;
using Serilog;

var webHost = Host
	.CreateDefaultBuilder(args)
	.ConfigureWebHostDefaults(webHostBuilder => webHostBuilder.UseStartup<Startup>())
	.UseSerilog((context, _, loggerConfiguration) => loggerConfiguration
		.ReadFrom.Configuration(context.Configuration))
	.Build();

await webHost.RunAsync();