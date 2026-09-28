using FoodOrderingApi.Data;
using FoodOrderingApi.Models.Orders;

namespace FoodOrderingApi.Services.Orders;

public class OrderItemAddOnService
{
    private readonly AppDbContext _db;
    public OrderItemAddOnService(AppDbContext db)
    {
        _db = db;
    }

    public List<OrderItemAddOn>? GetAll(int orderItemId)
    {
        bool orderItemExists = _db.OrderItems.Any(oi => oi.Id == orderItemId);
        if (!orderItemExists)
            return null;

        return _db.OrderItemAddOns
            .Where(oi => oi.OrderItemId == orderItemId)
            .ToList();
    }

    public OrderItemAddOn? Get(int id, int orderItemId)
    {
        OrderItemAddOn? orderItemAddOn = _db.OrderItemAddOns.Find(id);

        if (orderItemAddOn == null || orderItemAddOn.OrderItemId != orderItemId)
            return null;

        return orderItemAddOn;
    }
}