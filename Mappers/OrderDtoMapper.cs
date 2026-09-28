
using FoodOrderingApi.Dtos.Orders;
using FoodOrderingApi.Models.Orders;

namespace FoodOrderingApi.Mappers;
public static class OrderDtoMapper
{
    // To use this, tables must be joined:
            // Order? order = _db.Orders
            // .Include(o => o.OrderItems)
            // .ThenInclude(oi => oi.OrderItemAddOns)
            // .FirstOrDefault(o => o.Id  == id);
   public static GetOrderDto MapToDto(Order order)
    {
        return new GetOrderDto(
            Id: order.Id,
            PlaceId:   order.PlaceId,
            Status:    order.Status,
            CreatedAt: order.CreatedAt,
            OrderItems: order.OrderItems.Select(item => new GetOrderItemDto(
                Id: item.Id,
                ProductId: item.ProductId,
                Name:      item.Name,
                Price:     item.Price,
                Quantity:  item.Quantity,
                OrderItemAddOns: item.OrderItemAddOns.Select(addOn => new GetOrderItemAddOnDto(
                    Id: addOn.Id,
                    AddOnId:  addOn.AddOnId,
                    Name:     addOn.Name,
                    Price:    addOn.Price,
                    Quantity: addOn.Quantity
                )).ToList()
            )).ToList()
        );
    } 
}
    