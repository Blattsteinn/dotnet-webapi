using FoodOrderingApi.Data;
using FoodOrderingApi.Models.Orders;

namespace FoodOrderingApi.Services.Orders;

public class OrderItemService
{
    private readonly AppDbContext _db;
    public OrderItemService(AppDbContext db)
    {
        _db = db;
    }

    public List<OrderItem>? GetAll(int orderId)
    {
        bool orderExists = _db.Orders.Any(o => o.Id == orderId);
        if (!orderExists)
            return null;

        return _db.OrderItems
            .Where(oi => oi.OrderId == orderId)
            .ToList();
    }

    public OrderItem? Get(int id, int orderId)
    {
        OrderItem? orderItem = _db.OrderItems.Find(id);

        if (orderItem == null || orderItem.OrderId != orderId)
            return null;

        return orderItem;
    }

}