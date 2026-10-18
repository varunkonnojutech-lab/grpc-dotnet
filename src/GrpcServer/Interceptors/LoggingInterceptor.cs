using Grpc.Core;
using Grpc.Core.Interceptors;

namespace GrpcServer.Interceptors;

/// <summary>
/// Logs the start, successful completion, and failure of every gRPC call.
/// Registered globally via AddGrpc(options => options.Interceptors.Add&lt;LoggingInterceptor&gt;()).
/// </summary>
public class LoggingInterceptor : Interceptor
{
    private readonly ILogger<LoggingInterceptor> _logger;

    public LoggingInterceptor(ILogger<LoggingInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        _logger.LogInformation("gRPC call started: {Method}", context.Method);

        try
        {
            var response = await continuation(request, context);
            _logger.LogInformation("gRPC call finished: {Method}", context.Method);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "gRPC call failed: {Method}", context.Method);
            throw;
        }
    }
}
