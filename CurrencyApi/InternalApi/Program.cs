using InternalApi;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var webHost = WebHost
    .CreateDefaultBuilder(args)
    .UseKestrel(
        (builderContext, options) =>
        {
            var grpcPort = builderContext.Configuration.GetValue<int>("GrpcPort");
    
            options.ConfigureEndpointDefaults(
                p => { p.Protocols = p.IPEndPoint!.Port == grpcPort ? HttpProtocols.Http2 : HttpProtocols.Http1; });
            options.ConfigureEndpointDefaults(p => { p.Protocols = HttpProtocols.Http2; });
            // options.ListenLocalhost(grpcPort, o => o.Protocols = HttpProtocols.Http2);
            // options.ListenLocalhost(5242, o => o.Protocols = HttpProtocols.Http1);
        })
    .UseStartup<Startup>()
    .Build();

await webHost.RunAsync();