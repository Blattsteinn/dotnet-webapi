

using Microsoft.EntityFrameworkCore;

using FoodOrderingApi.Data;
using FoodOrderingApi.Models.Orders;
using FoodOrderingApi.Models.Places;
using FoodOrderingApi.Dtos.OrderItemAddOn;
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
        bool addOnExists = _db.OrderItems.Any(oi => oi.Id == orderItemId);
        if (!addOnExists)
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

    public OrderItemAddOn? Add(CreateOrderItemAddOnDto createOrderItemAddOnDto, int orderItemId)
    {
        AddOn? addOn = _db.AddOns.Find(createOrderItemAddOnDto.AddOnId);
        OrderItem? orderItem = _db.OrderItems.Find(orderItemId);

        if(addOn == null)
            return null;
        
        if(orderItem == null)
            return null;

        OrderItemAddOn orderItemAddOn = new OrderItemAddOn
        {
            OrderItemId = orderItemId,
            AddOnId = createOrderItemAddOnDto.AddOnId,
            Name = addOn.Name,
            Price = addOn.Price,
            Quantity = createOrderItemAddOnDto.Quantity
        };

        _db.OrderItemAddOns.Add(orderItemAddOn);
        _db.SaveChanges();

        return orderItemAddOn;
    }
}