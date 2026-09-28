using FoodOrderingApi.Data;

using FoodOrderingApi.Models.Orders;
using FoodOrderingApi.Models.Places;

using FoodOrderingApi.Dtos.Orders;
using FoodOrderingApi.Mappers;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderingApi.Services.Orders;

public enum AddOrderResult
{
    Success,
    PlaceNotFound,
    ZeroProducts,
    ProductNotFound,
    AddOnNotFound,
}

public class OrderService
{
    private readonly AppDbContext _db;
    public OrderService(AppDbContext db)
    {
        _db = db;
    }

    // Does not expect a full list.
    public List<Order> GetAll()
    {
        return _db.Orders.ToList();
    }

    public GetOrderDto? Get(int id)
    {   
        Order? order = _db.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.OrderItemAddOns)
            .FirstOrDefault(o => o.Id  == id);
            
        if (order == null)
            return null;

        return OrderDtoMapper.MapToDto(order);
    }

    public (GetOrderDto?, AddOrderResult) Add(CreateOrderDto orderDto)
    {   

        /// CLIENT SENDS: CreateOrderDto
        /// 
        /// We construct Order, OrderItem, OrderItemAddOn 
        /// as we loop through IDs in CreateOrderDto
        /// 
        /// RETURN VALUE: OrderDto

        int placeId = orderDto.PlaceId;

        // If place does not exist return early
        Place? place = _db.Places.Find(placeId);
        if(place == null)
            return (null, AddOrderResult.PlaceNotFound);
        
        // If no products return early
        List<CreateOrderItemDto> orderItemDto = orderDto.OrderItems;
        if(orderItemDto.Count == 0)
            return(null, AddOrderResult.ZeroProducts);
        
        var newOrder = new Order
        {
            PlaceId = placeId,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };


        foreach(var orderItem in orderItemDto)
        {   
            // If product does not exist return early
            Product? product = _db.Products.Find(orderItem.ProductId);
            if(product == null)
                return (null, AddOrderResult.ProductNotFound);

            var newOrderItem = new OrderItem
            {
                ProductId = product.Id,
                Name = product.Name,
                Price = product.Price,
                Quantity = orderItem.Quantity
            };


            List<CreateOrderItemAddOnDto> orderItemAddOnsDto = orderItem.AddOns;
            foreach (var addOnDto in orderItemAddOnsDto)
            {
                // If addOn does not exist return early
                AddOn? addOn = _db.AddOns.Find(addOnDto.AddOnId);
                if(addOn == null)
                    return (null, AddOrderResult.AddOnNotFound);
                
                var newOrderItemAddOn = new OrderItemAddOn
                {
                    AddOnId = addOn.Id,
                    Name = addOn.Name,
                    Price = addOn.Price,
                    Quantity = addOnDto.Quantity
                };

                newOrderItem.OrderItemAddOns.Add(newOrderItemAddOn);
            }

            newOrder.OrderItems.Add(newOrderItem);
        }

        _db.Orders.Add(newOrder);
        _db.SaveChanges();
        return (OrderDtoMapper.MapToDto(newOrder), AddOrderResult.Success);
  
    }

    public GetOrderDto? Update(int id, UpdateOrderDto orderDto)
    {
        Order? order = _db.Orders
             .Include(o => o.OrderItems)
             .ThenInclude(oi => oi.OrderItemAddOns)
             .FirstOrDefault(o => o.Id  == id);

        if (order == null)
            return null;

        if(orderDto.Status.HasValue)
            order.Status = orderDto.Status.Value;
        

        _db.SaveChanges();

        return OrderDtoMapper.MapToDto(order);
    }

}