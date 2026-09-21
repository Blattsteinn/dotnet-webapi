

using Microsoft.EntityFrameworkCore;

using FoodOrderingApi.Data;
using FoodOrderingApi.Models.Orders;
using FoodOrderingApi.Models.Places;
using FoodOrderingApi.Dtos.Orders;

namespace FoodOrderingApi.Services.Orders;

public enum AddOrderItemResult
{
    Success,
    OrderNotFound,
    ProductNotFound
}

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

    public (AddOrderItemResult Result, OrderItem? OrderItem) Add(CreateOrderItemDto orderItemDto, int orderId)
    {
        Order? order = _db.Orders.Find(orderId);
        if (order == null)
            return (AddOrderItemResult.OrderNotFound, null);

        Product? product = _db.Products.Find(orderItemDto.ProductId);
        if (product == null)
            return (AddOrderItemResult.ProductNotFound, null);

        OrderItem orderItem = new OrderItem
        {
            OrderId = orderId,
            ProductId = product.Id,
             // snapshot values
            Name = product.Name,
            Price = product.Price,
            Quantity = orderItemDto.Quantity
        };

        _db.OrderItems.Add(orderItem);
        _db.SaveChanges();

        return (AddOrderItemResult.Success, orderItem);
    }
}