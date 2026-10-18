using GrpcServer.Interceptors;
using GrpcServer.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// gRPC requires HTTP/2. For local development we use cleartext HTTP/2
// on a dedicated port (no TLS certificate needed).
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5001, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

builder.Services.AddSingleton<OrderStore>();
builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<LoggingInterceptor>();
});

var app = builder.Build();

app.MapGrpcService<OrderGrpcService>();
app.MapGet("/", () => "gRPC server is running. Use a gRPC client to call OrderService.");

app.Run();
