using System.Collections.Concurrent;
using GrpcServer.Protos;

namespace GrpcServer.Services;

/// <summary>
/// Thread-safe in-memory order storage seeded with sample data.
/// </summary>
public class OrderStore
{
    private readonly ConcurrentDictionary<int, Order> _orders = new();
    private int _nextId = 3;

    public OrderStore()
    {
        _orders[1] = new Order { Id = 1, ProductName = "Laptop", Quantity = 2, Status = "Shipped", Total = 2400.00 };
        _orders[2] = new Order { Id = 2, ProductName = "Keyboard", Quantity = 5, Status = "Pending", Total = 375.50 };
        _orders[3] = new Order { Id = 3, ProductName = "Monitor", Quantity = 1, Status = "Delivered", Total = 349.99 };
    }

    public Order? Get(int id) =>
        _orders.TryGetValue(id, out var order) ? order : null;

    public IEnumerable<Order> List(string? statusFilter) =>
        string.IsNullOrWhiteSpace(statusFilter)
            ? _orders.Values.OrderBy(o => o.Id)
            : _orders.Values
                .Where(o => o.Status.Equals(statusFilter, StringComparison.OrdinalIgnoreCase))
                .OrderBy(o => o.Id);

    public Order Add(string productName, int quantity, double unitPrice)
    {
        var id = Interlocked.Increment(ref _nextId);
        var order = new Order
        {
            Id = id,
            ProductName = productName,
            Quantity = quantity,
            Status = "Pending",
            Total = quantity * unitPrice
        };
        _orders[id] = order;
        return order;
    }
}
