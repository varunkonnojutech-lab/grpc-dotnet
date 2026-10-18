using Grpc.Core;
using Grpc.Net.Client;
using GrpcServer.Protos;

// The server listens on cleartext HTTP/2 at http://localhost:5001.
using var channel = GrpcChannel.ForAddress("http://localhost:5001");
var client = new OrderService.OrderServiceClient(channel);

Console.WriteLine("=== 1. Unary: GetOrder ===");
try
{
    var order = await client.GetOrderAsync(new GetOrderRequest { Id = 1 });
    Print(order);
}
catch (RpcException ex)
{
    Console.WriteLine($"RPC failed: {ex.StatusCode} - {ex.Status.Detail}");
}

Console.WriteLine();
Console.WriteLine("=== 2. Server streaming: ListOrders ===");
using (var stream = client.ListOrders(new ListOrdersRequest { StatusFilter = "" }))
{
    await foreach (var order in stream.ResponseStream.ReadAllAsync())
    {
        Print(order);
    }
}

Console.WriteLine();
Console.WriteLine("=== 3. Unary: PlaceOrder ===");
var placed = await client.PlaceOrderAsync(new PlaceOrderRequest
{
    ProductName = "Headphones",
    Quantity = 3,
    UnitPrice = 89.99
});
Print(placed);

Console.WriteLine();
Console.WriteLine("=== 4. Bidirectional streaming: TrackOrders ===");
using var duplex = client.TrackOrders();

var readTask = Task.Run(async () =>
{
    await foreach (var update in duplex.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine($"[update] order {update.OrderId}: {update.Status} — {update.Message}");
    }
});

foreach (var id in new[] { 1, 2, 999 })
{
    await duplex.RequestStream.WriteAsync(new TrackRequest { OrderId = id });
    await Task.Delay(300);
}

await duplex.RequestStream.CompleteAsync();
await readTask;

Console.WriteLine();
Console.WriteLine("Done.");

static void Print(Order o) =>
    Console.WriteLine($"Order {o.Id}: {o.ProductName} x{o.Quantity} — {o.Status} — total ${o.Total:F2}");
