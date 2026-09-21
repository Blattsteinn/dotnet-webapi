

using FoodOrderingApi.Data;
using FoodOrderingApi.Models;
using FoodOrderingApi.Dtos.Order;

namespace FoodOrderingApi.Services;

public class OrderService
{
    private readonly AppDbContext _db;
    public OrderService(AppDbContext db)
    {
        _db = db;
    }

    // GET, GET ID, POST, PATCH -> USES UpdateOrderDto.cs

    public List<Order> GetAll()
    {
        return _db.Orders.ToList();
    }

    public Order? Get(int id)
    {
        return _db.Orders.Find(id);
    }

    public Order? Add(Order order)
    {
        Place? place = _db.Places.Find(order.PlaceId);
        if (place == null)
            return null;

        order.Status = OrderStatus.Pending;
        order.CreatedAt = DateTime.UtcNow;

        _db.Orders.Add(order);
        _db.SaveChanges();

        return order;
    }

    public Order? Update(int id, UpdateOrderDto orderDto)
    {
        Order? order = _db.Orders.Find(id);
        if (order == null)
            return null;

        order.Status = orderDto.Status;

        _db.SaveChanges();

        return order;
    }
}