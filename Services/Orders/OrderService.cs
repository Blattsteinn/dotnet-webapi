

using FoodOrderingApi.Data;
using FoodOrderingApi.Models.Orders;
using FoodOrderingApi.Models.Places;
using FoodOrderingApi.Dtos.Orders;

namespace FoodOrderingApi.Services.Orders;

public class OrderService
{
    private readonly AppDbContext _db;
    public OrderService(AppDbContext db)
    {
        _db = db;
    }


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
        // can be null so whatever uhh
        if (order.PlaceId is not int placeId)
            return null;

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

        if(orderDto.Status.HasValue)
            order.Status = orderDto.Status.Value;
        

        _db.SaveChanges();

        return order;
    }
}