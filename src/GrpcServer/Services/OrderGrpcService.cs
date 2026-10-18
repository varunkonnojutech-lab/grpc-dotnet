using Grpc.Core;
using GrpcServer.Protos;

namespace GrpcServer.Services;

public class OrderGrpcService : OrderService.OrderServiceBase
{
    private readonly OrderStore _store;
    private readonly ILogger<OrderGrpcService> _logger;

    public OrderGrpcService(OrderStore store, ILogger<OrderGrpcService> logger)
    {
        _store = store;
        _logger = logger;
    }

    // Unary
    public override Task<Order> GetOrder(GetOrderRequest request, ServerCallContext context)
    {
        var order = _store.Get(request.Id);
        if (order is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Order {request.Id} not found."));
        }

        return Task.FromResult(order);
    }

    // Server streaming
    public override async Task ListOrders(
        ListOrdersRequest request,
        IServerStreamWriter<Order> responseStream,
        ServerCallContext context)
    {
        foreach (var order in _store.List(request.StatusFilter))
        {
            if (context.CancellationToken.IsCancellationRequested)
            {
                break;
            }

            await responseStream.WriteAsync(order);
            // Small delay to make the streaming visible in the client demo.
            await Task.Delay(200, context.CancellationToken);
        }
    }

    // Unary
    public override Task<Order> PlaceOrder(PlaceOrderRequest request, ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.ProductName) || request.Quantity <= 0)
        {
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "ProductName is required and Quantity must be positive."));
        }

        return Task.FromResult(_store.Add(request.ProductName, request.Quantity, request.UnitPrice));
    }

    // Bidirectional streaming
    public override async Task TrackOrders(
        IAsyncStreamReader<TrackRequest> requestStream,
        IServerStreamWriter<TrackingUpdate> responseStream,
        ServerCallContext context)
    {
        await foreach (var req in requestStream.ReadAllAsync(context.CancellationToken))
        {
            var order = _store.Get(req.OrderId);

            var update = order is null
                ? new TrackingUpdate
                {
                    OrderId = req.OrderId,
                    Status = "Unknown",
                    Message = $"No order with id {req.OrderId}."
                }
                : new TrackingUpdate
                {
                    OrderId = order.Id,
                    Status = order.Status,
                    Message = $"Order {order.Id} ({order.ProductName}) is {order.Status}."
                };

            await responseStream.WriteAsync(update);
        }

        _logger.LogInformation("TrackOrders stream completed by client.");
    }
}
