using FoodOrderingApi.Data;

using FoodOrderingApi.Models.Orders;
using FoodOrderingApi.Models.Places;

using FoodOrderingApi.Dtos.Orders;
using FoodOrderingApi.Mappers;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FoodOrderingApi.Services.Orders;

public enum AddOrderResult
{
    Success,
    PlaceNotFound,
    ZeroProducts,
    ProductNotFound,
    AddOnNotFound,
    RequiredAddOnsNotSpecified,
    SingleChoiceAddOnsExceeded,
}

// ---------------------------------------------------------------------------
// ---------------------------------------------------------------------------
// TWO SCOPES:
// 1. Customer
// 2. Place Owner
// ---------------------------------------------------------------------------
public class OrderService
{
    private readonly AppDbContext _db;
    public OrderService(AppDbContext db)
    {
        _db = db;
    }

    private IQueryable<Order> WithItems() =>
        _db.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.OrderItemAddOns);

    private bool OwnerHoldsPlace(string ownerId, int placeId) =>
        _db.PlaceOwnerships.Any(o => o.PlaceId == placeId && o.OwnerId == ownerId);

    // ---------------------------------------------------------------------------
    // ---------------------------------------------------------------------------
    // CUSTOMER SCOPE
    // ---------------------------------------------------------------------------

    public List<GetOrderDto> GetForUser(string userId)
    {
        return WithItems()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToList()
            .Select(OrderDtoMapper.MapToDto)
            .ToList();
    }

    // Returns null when the order is missing OR when it belongs to
    // somebody else.
    public GetOrderDto? GetForUser(string userId, int orderId)
    {
        Order? order = WithItems()
            .FirstOrDefault(o => o.Id == orderId && o.UserId == userId);

        return order == null ? null : OrderDtoMapper.MapToDto(order);
    }

    // userId here comes from the caller's token.
    public (GetOrderDto?, AddOrderResult) Add(CreateOrderDto orderDto, string? userId)
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
            UserId = userId,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };


        foreach(var orderItem in orderItemDto)
        {   
            // If product does not exist return early
            Product? product = _db.Products
                .Include(p => p.AddOnCategories)
                .FirstOrDefault(p => p.Id == orderItem.ProductId);
            if(product == null)
                return (null, AddOrderResult.ProductNotFound);

            var newOrderItem = new OrderItem
            {
                ProductId = product.Id,
                Name = product.Name,
                Price = product.Price,
                Quantity = orderItem.Quantity
            };

            // Each product has addOnCategories. Categories having Required must
            // have at least one add-on chosen, and categories that are not
            // Multichoice accept at most one add-on.
            var requiredCategories = product.AddOnCategories
                .Where(c => c.Required)
                .Select(c => c.Id)
                .ToHashSet();

            var singleChoiceCategories = product.AddOnCategories
                .Where(c => !c.Multichoice)
                .Select(c => c.Id)
                .ToHashSet();

            var chosenPerCategory = new Dictionary<int, int>();

            List<CreateOrderItemAddOnDto> orderItemAddOnsDto = orderItem.AddOns;
            foreach (var addOnDto in orderItemAddOnsDto)
            {
                // If addOn does not exist return early
                AddOn? addOn = _db.AddOns.Find(addOnDto.AddOnId);
                if(addOn == null)
                    return (null, AddOrderResult.AddOnNotFound);

                int addOnCategoryId = addOn.AddOnCategoryId;

                chosenPerCategory.TryGetValue(addOnCategoryId, out int chosen);
                chosen++;
                chosenPerCategory[addOnCategoryId] = chosen;

                // A category that is not Multichoice accepts a single add-on.
                if (singleChoiceCategories.Contains(addOnCategoryId) && chosen > 1)
                {
                    return (null, AddOrderResult.SingleChoiceAddOnsExceeded);
                }

                var newOrderItemAddOn = new OrderItemAddOn
                {
                    AddOnId = addOn.Id,
                    Name = addOn.Name,
                    Price = addOn.Price,
                    Quantity = addOnDto.Quantity
                };

                newOrderItem.OrderItemAddOns.Add(newOrderItemAddOn);
            }
            
            // Check if required addOns are here
            if (!requiredCategories.IsSubsetOf(chosenPerCategory.Keys))
            {
                return (null, AddOrderResult.RequiredAddOnsNotSpecified);
            }

            newOrder.OrderItems.Add(newOrderItem);
 
        }

        _db.Orders.Add(newOrder);
        _db.SaveChanges();
        return (OrderDtoMapper.MapToDto(newOrder), AddOrderResult.Success);
  
    }
    

    // ---------------------------------------------------------------------------
    // ---------------------------------------------------------------------------
    // PLACE OWNER SCOPE
    // ---------------------------------------------------------------------------

    // null == the place does not exist, or does not belong to this owner.
    public List<GetOrderDto>? GetForOwner(string ownerId, int placeId)
    {
        if (!OwnerHoldsPlace(ownerId, placeId))
            return null;

        return WithItems()
            .Where(o => o.PlaceId == placeId)
            .OrderByDescending(o => o.CreatedAt)
            .ToList()
            .Select(OrderDtoMapper.MapToDto)
            .ToList();
    }

    public GetOrderDto? GetForOwner(string ownerId, int placeId, int orderId)
    {
        if (!OwnerHoldsPlace(ownerId, placeId))
            return null;

        Order? order = WithItems()
            .FirstOrDefault(o => o.Id == orderId && o.PlaceId == placeId);

        return order == null ? null : OrderDtoMapper.MapToDto(order);
    }

    // The contents of a placed order are immutable; only the status can change.
    public GetOrderDto? SetStatusForPlace(string ownerId, int placeId, int orderId, OrderStatus status)
    {
        if (!OwnerHoldsPlace(ownerId, placeId))
            return null;

        Order? order = WithItems()
            .FirstOrDefault(o => o.Id == orderId && o.PlaceId == placeId);

        if (order == null)
            return null;

        order.Status = status;
        _db.SaveChanges();

        return OrderDtoMapper.MapToDto(order);
    }

}